using System.Threading.Channels;
using System.Text.Json;
using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.Host.Messaging;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Pipeline;
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
    private static readonly DomainEventTypeRegistry EventTypeRegistry =
        DomainEventTypeRegistry.CreateFrom(typeof(DrinkCreatedEvent).Assembly);

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();

    public Task DisposeAsync() => fixture.ResetDatabaseAsync();

    [Fact]
    public async Task Worker_PublishesCommittedEvent_AndMarksRecordDispatched()
    {
        await using var harness = await DurableMessagingHarness.CreateAsync(fixture.PostgresConnectionString);

        var drinkId = await CreateDrinkAsync("Transport Publish");
        await RunWorkerUntilAsync(harness.PublisherBus, async () =>
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
    public async Task PublishedContractEvent_UsesLogicalNameAndExpectedPayloadShape()
    {
        await using var harness = await DurableMessagingHarness.CreateAsync(fixture.PostgresConnectionString);

        var drinkId = await CreateDrinkAsync("Payload Shape");
        await RunWorkerUntilAsync(harness.PublisherBus, async () =>
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

        using var unsubscribedBus = CreateUnsubscribedPublisherBus();
        await RunWorkerUntilAsync(unsubscribedBus, async () =>
        {
            await using var dbContext = fixture.CreateDbContext();
            await WaitForDispatchAsync(dbContext, drinkId);
        });

        await using var verificationContext = fixture.CreateDbContext();
        var record = await verificationContext.DomainEventRecords.SingleAsync(r => r.AggregateId == drinkId);
        record.DispatchedAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task PublishFailure_LeavesRowRetryable_AndLaterRunPublishesSuccessfully()
    {
        await using var harness = await DurableMessagingHarness.CreateAsync(fixture.PostgresConnectionString);
        var drinkId = await CreateDrinkAsync("Retryable After Failure");

        var failingBus = Substitute.For<IBus>();
        failingBus.Publish(Arg.Any<object>()).Returns(_ => throw new InvalidOperationException("simulated transport failure"));
        await RunWorkerUntilAsync(
            failingBus,
            async () => await Task.Delay(500),
            TimeSpan.FromSeconds(2));

        await using (var afterFailureContext = fixture.CreateDbContext())
        {
            var failedRecord = await afterFailureContext.DomainEventRecords.SingleAsync(r => r.AggregateId == drinkId);
            failedRecord.DispatchedAtUtc.ShouldBeNull();
        }

        await RunWorkerUntilAsync(harness.PublisherBus, async () =>
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

    private async Task<Guid> CreateDrinkAsync(string name)
    {
        await using var dbContext = fixture.CreateDbContext();
        var drink = Drink.Create(DrinkName.Create(name), null, ImageUrl.Create(null));
        dbContext.Drinks.Add(drink);
        await dbContext.SaveChangesAsync();

        return drink.Id;
    }

    private async Task RunWorkerUntilAsync(
        IBus bus,
        Func<Task> assertion,
        TimeSpan? timeout = null)
    {
        using var serviceProvider = new ServiceCollection()
            .AddScoped(_ => fixture.CreateDbContext())
            .AddScoped<DrinkCatalogDbContext>(_ => fixture.CreateDbContext())
            .BuildServiceProvider();

        var worker = new OutboxWorker(
            serviceProvider,
            [new OutboxSourceDescriptor("drink-catalog", typeof(DrinkCatalogDbContext))],
            EventTypeRegistry,
            bus,
            Microsoft.Extensions.Options.Options.Create(
                new OutboxWorkerOptions { BatchSize = 10, PollInterval = TimeSpan.FromMilliseconds(100) }),
            NullLogger<OutboxWorker>.Instance);

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

    private IBus CreateUnsubscribedPublisherBus()
    {
        var activator = new BuiltinHandlerActivator();

        return Configure.With(activator)
            .Transport(t => t.UsePostgreSqlAsOneWayClient(
                fixture.PostgresConnectionString,
                "rebus_messages",
                expiredMessagesCleanupInterval: null,
                schemaName: "messaging"))
            .Subscriptions(s => s.StoreInPostgres(
                fixture.PostgresConnectionString,
                "rebus_subscriptions",
                isCentralized: true,
                automaticallyCreateTables: true,
                additionalConnectionSetup: null))
            .Serialization(ConfigureSerialization)
            .Options(ConfigureOptions)
            .Start();
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

    private sealed class DurableMessagingHarness : IAsyncDisposable
    {
        private readonly BuiltinHandlerActivator _publisherActivator;
        private readonly BuiltinHandlerActivator _subscriberActivator = new();
        private readonly Channel<ReceivedTransportMessage> _messages = Channel.CreateUnbounded<ReceivedTransportMessage>();

        private DurableMessagingHarness(BuiltinHandlerActivator publisherActivator, IBus publisherBus)
        {
            _publisherActivator = publisherActivator;
            PublisherBus = publisherBus;
        }

        public IBus PublisherBus { get; }

        public static async Task<DurableMessagingHarness> CreateAsync(string connectionString)
        {
            var publisherActivator = new BuiltinHandlerActivator();
            var publisherBus = Configure.With(publisherActivator)
                .Transport(t => t.UsePostgreSqlAsOneWayClient(
                    connectionString,
                    "rebus_messages",
                    expiredMessagesCleanupInterval: null,
                    schemaName: "messaging"))
                .Subscriptions(s => s.StoreInPostgres(
                    connectionString,
                    "rebus_subscriptions",
                    isCentralized: true,
                    automaticallyCreateTables: true,
                    additionalConnectionSetup: null))
                .Serialization(ConfigureSerialization)
                .Options(ConfigureOptions)
                .Start();

            var harness = new DurableMessagingHarness(publisherActivator, publisherBus);

            harness._subscriberActivator
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

            var subscriberBus = Configure.With(harness._subscriberActivator)
                .Transport(t => t.UsePostgreSql(
                    connectionString,
                    "rebus_messages",
                    $"verify-dispatch-{Guid.NewGuid():N}",
                    expiredMessagesCleanupInterval: null,
                    schemaName: "messaging"))
                .Subscriptions(s => s.StoreInPostgres(
                    connectionString,
                    "rebus_subscriptions",
                    isCentralized: true,
                    automaticallyCreateTables: true,
                    additionalConnectionSetup: null))
                .Serialization(ConfigureSerialization)
                .Options(ConfigureOptions)
                .Start();

            await subscriberBus.Subscribe<DrinkCreatedEvent>();

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
            _publisherActivator.Dispose();
            _subscriberActivator.Dispose();
            _messages.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }
    }

    private sealed record ReceivedTransportMessage(
        DrinkCreatedEvent Event,
        IReadOnlyDictionary<string, string> Headers,
        byte[] Body);
}
