using System.Threading.Channels;
using System.Text.Json;
using AlCopilot.DrinkCatalog;
using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.Host.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.RabbitMq;
using Rebus.Serialization;
using Rebus.Serialization.Custom;
using Rebus.Serialization.Json;
using Rebus.Topic;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AlCopilot.Host.Tests;

[Collection("DurableMessaging")]
public sealed class DurableMessagingTransportIntegrationTests(DurableMessagingFixture fixture) : IAsyncLifetime
{
    private static readonly AlCopilot.Shared.Domain.DomainEventTypeRegistry EventTypeRegistry =
        AlCopilot.Shared.Domain.DomainEventTypeRegistry.CreateFrom(typeof(DrinkCreatedEvent).Assembly);

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();

    public Task DisposeAsync() => fixture.ResetDatabaseAsync();

    [Fact]
    public async Task Worker_PublishesCommittedEvent_ThroughRabbitMq_AndMarksRecordDispatched()
    {
        await using var harness = await DurableMessagingHarness.CreateAsync(fixture.MessagingConnectionString);
        var drinkId = await CreateDrinkAsync("Transport Publish");
        await using var provider = BuildServiceProvider();
        using var worker = CreateWorker(provider);

        await RunWorkerUntilAsync(worker, async () =>
        {
            await using var dbContext = fixture.CreateDbContext();
            await WaitForDispatchAsync(dbContext, drinkId);
        });

        var receivedMessage = await harness.ReceiveAsync();
        receivedMessage.ShouldNotBeNull();

        await using var verificationContext = fixture.CreateDbContext();
        var record = await verificationContext.DomainEventRecords.SingleAsync(r => r.AggregateId == drinkId);
        record.DispatchedAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task PublishedContractEvent_UsesLogicalNameAndExpectedPayloadShape_ThroughRabbitMq()
    {
        await using var harness = await DurableMessagingHarness.CreateAsync(fixture.MessagingConnectionString);
        var drinkId = await CreateDrinkAsync("Payload Shape");
        await using var provider = BuildServiceProvider();
        using var worker = CreateWorker(provider);

        await RunWorkerUntilAsync(worker, async () =>
        {
            await using var dbContext = fixture.CreateDbContext();
            await WaitForDispatchAsync(dbContext, drinkId);
        });

        var receivedMessage = await harness.ReceiveAsync();

        receivedMessage.ShouldNotBeNull();
        receivedMessage!.Headers["rbs2-msg-type"].ShouldBe("drink-catalog.drink-created.v1");

        var contractEvent = JsonSerializer.Deserialize<DrinkCreatedEvent>(receivedMessage.Body);
        contractEvent.ShouldNotBeNull();
        contractEvent!.DrinkId.ShouldBe(drinkId);
    }

    [Fact]
    public async Task Worker_MarksRecordDispatched_WhenNoSubscriberExists()
    {
        var drinkId = await CreateDrinkAsync("Retry Publish");
        await using var provider = BuildServiceProvider();
        using var worker = CreateWorker(provider);

        await RunWorkerUntilAsync(worker, async () =>
        {
            await using var dbContext = fixture.CreateDbContext();
            await WaitForDispatchAsync(dbContext, drinkId);
        });

        await using var verificationContext = fixture.CreateDbContext();
        var record = await verificationContext.DomainEventRecords.SingleAsync(r => r.AggregateId == drinkId);
        record.DispatchedAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task PublishFailure_LeavesRowRetryable_AndLaterRunPublishesSuccessfully_ThroughRabbitMq()
    {
        await using var harness = await DurableMessagingHarness.CreateAsync(fixture.MessagingConnectionString);
        var drinkId = await CreateDrinkAsync("Retryable After Failure");

        var failingBus = Substitute.For<IBus>();
        failingBus.Publish(Arg.Any<object>()).Returns(_ => throw new InvalidOperationException("simulated transport failure"));
        await using (var failingProvider = BuildServiceProvider())
        {
            using var failingWorker = CreateWorker(failingProvider, failingBus);
            await RunWorkerUntilAsync(
                failingWorker,
                async () => await Task.Delay(500),
                TimeSpan.FromSeconds(2));
        }

        await using (var afterFailureContext = fixture.CreateDbContext())
        {
            var failedRecord = await afterFailureContext.DomainEventRecords.SingleAsync(r => r.AggregateId == drinkId);
            failedRecord.DispatchedAtUtc.ShouldBeNull();
        }

        await using (var provider = BuildServiceProvider())
        {
            using var worker = CreateWorker(provider);
            await RunWorkerUntilAsync(worker, async () =>
            {
                await using var dbContext = fixture.CreateDbContext();
                await WaitForDispatchAsync(dbContext, drinkId);
            });
        }

        var receivedMessage = await harness.ReceiveAsync();
        receivedMessage.ShouldNotBeNull();

        await using var verificationContext = fixture.CreateDbContext();
        var record = await verificationContext.DomainEventRecords.SingleAsync(r => r.AggregateId == drinkId);
        record.DispatchedAtUtc.ShouldNotBeNull();
    }

    private async Task<Guid> CreateDrinkAsync(string name)
    {
        await using var dbContext = fixture.CreateDbContext();
        var drink = Drink.Create(DrinkName.Create(name), null, ImageUrl.Create(null));
        dbContext.Drinks.Add(drink);
        await dbContext.SaveChangesAsync();

        return drink.Id;
    }

    private ServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:drink-catalog"] = fixture.PostgresConnectionString,
                ["ConnectionStrings:messaging"] = fixture.MessagingConnectionString,
                ["Messaging:Transport"] = "RabbitMq",
                [$"{OutboxWorkerOptions.SectionName}:BatchSize"] = "10",
                [$"{OutboxWorkerOptions.SectionName}:PollInterval"] = "00:00:00.100"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddDrinkCatalogModule(configuration);
        services.AddDurableMessaging(configuration);

        return services.BuildServiceProvider();
    }

    private static OutboxWorker CreateWorker(IServiceProvider provider, IBus? bus = null)
    {
        return new OutboxWorker(
            provider,
            provider.GetServices<AlCopilot.Shared.Data.OutboxSourceDescriptor>(),
            provider.GetRequiredService<AlCopilot.Shared.Domain.DomainEventTypeRegistry>(),
            bus ?? provider.GetRequiredService<IBus>(),
            provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OutboxWorkerOptions>>(),
            NullLogger<OutboxWorker>.Instance);
    }

    private static async Task RunWorkerUntilAsync(
        OutboxWorker worker,
        Func<Task> assertion,
        TimeSpan? timeout = null)
    {

        using var cancellationTokenSource = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(20));
        await worker.StartAsync(cancellationTokenSource.Token);

        try
        {
            await assertion();
        }
        finally
        {
            cancellationTokenSource.Cancel();
            await worker.StopAsync(CancellationToken.None);
        }
    }

    private static async Task WaitForDispatchAsync(DrinkCatalogDbContext dbContext, Guid drinkId)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(20);

        while (DateTimeOffset.UtcNow < deadline)
        {
            var record = await dbContext.DomainEventRecords
                .SingleAsync(r => r.AggregateId == drinkId);

            if (record.DispatchedAtUtc is not null)
            {
                return;
            }

            dbContext.ChangeTracker.Clear();
            await Task.Delay(200);
        }

        throw new TimeoutException("Timed out waiting for dispatched outbox record.");
    }

    private sealed class DurableMessagingHarness : IAsyncDisposable
    {
        private readonly BuiltinHandlerActivator _subscriberActivator;
        private readonly Channel<ReceivedTransportMessage> _messages = Channel.CreateUnbounded<ReceivedTransportMessage>();

        private DurableMessagingHarness(BuiltinHandlerActivator subscriberActivator, IBus subscriberBus)
        {
            _subscriberActivator = subscriberActivator;
            SubscriberBus = subscriberBus;
        }

        public IBus SubscriberBus { get; }

        public static async Task<DurableMessagingHarness> CreateAsync(string connectionString)
        {
            var queueName = $"verify-dispatch-{Guid.NewGuid():N}";
            var subscriberActivator = new BuiltinHandlerActivator();
            var harness = new DurableMessagingHarness(subscriberActivator, default!);

            subscriberActivator
                .Handle<DrinkCreatedEvent>(async message =>
                {
                    var context = MessageContext.Current
                        ?? throw new InvalidOperationException("No Rebus message context was available.");

                    await harness._messages.Writer.WriteAsync(
                        new ReceivedTransportMessage(
                            message,
                            new Dictionary<string, string>(context.Headers),
                            [.. context.TransportMessage.Body]));
                });

            var subscriberBus = Configure.With(subscriberActivator)
                .Transport(t => t.UseRabbitMq(connectionString, queueName))
                .Serialization(ConfigureSerialization)
                .Options(ConfigureOptions)
                .Start();

            harness = new DurableMessagingHarness(subscriberActivator, subscriberBus);
            await harness.SubscriberBus.Subscribe<DrinkCreatedEvent>();

            return harness;
        }

        public async Task<ReceivedTransportMessage?> ReceiveAsync(TimeSpan? timeout = null)
        {
            using var cancellationTokenSource = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(20));

            try
            {
                return await _messages.Reader.ReadAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        public ValueTask DisposeAsync()
        {
            _subscriberActivator.Dispose();
            _messages.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }
    }

    private static void ConfigureSerialization(StandardConfigurer<ISerializer> serializationConfigurer)
    {
        serializationConfigurer.UseSystemTextJson();

        var typeNameBuilder = serializationConfigurer.UseCustomMessageTypeNames();
        foreach (var pair in EventTypeRegistry.GetTypeNames())
        {
            typeNameBuilder.AddWithCustomName(pair.Key, pair.Value);
        }
    }

    private static void ConfigureOptions(OptionsConfigurer optionsConfigurer)
    {
        optionsConfigurer.Register(_ => new LogicalMessageTypeNameConvention(EventTypeRegistry));
        optionsConfigurer.Decorate<ITopicNameConvention>(_ => new LogicalTopicNameConvention(EventTypeRegistry));
    }

    private sealed record ReceivedTransportMessage(
        DrinkCreatedEvent Event,
        IReadOnlyDictionary<string, string> Headers,
        byte[] Body);
}
