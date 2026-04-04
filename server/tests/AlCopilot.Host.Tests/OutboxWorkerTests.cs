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
using NSubstitute;
using Rebus.Bus;
using Shouldly;
using Xunit;

namespace AlCopilot.Host.Tests;

[Collection("DurableMessaging")]
public sealed class OutboxWorkerTests(DurableMessagingFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetDatabaseAsync();

    public Task DisposeAsync() => fixture.ResetDatabaseAsync();

    [Fact]
    public async Task Worker_PublishesPendingRecord_AndMarksItDispatched()
    {
        var bus = Substitute.For<IBus>();
        var recordId = await PersistDrinkCreatedEventAsync("Published By Worker");

        await RunWorkerAsync(bus, TimeSpan.FromSeconds(3), async () =>
        {
            await using var dbContext = fixture.CreateDbContext();
            return await dbContext.DomainEventRecords.AnyAsync(r => r.Id == recordId && r.DispatchedAtUtc != null);
        });

        await bus.Received(1).Publish(Arg.Is<DrinkCreatedEvent>(evt => evt.DrinkId != Guid.Empty));

        await using var verificationContext = fixture.CreateDbContext();
        var record = await verificationContext.DomainEventRecords.SingleAsync(r => r.Id == recordId);
        record.DispatchedAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task Worker_SkipsAlreadyDispatchedRecords()
    {
        var bus = Substitute.For<IBus>();
        var recordId = await PersistDrinkCreatedEventAsync("Already Dispatched");

        await using (var dbContext = fixture.CreateDbContext())
        {
            var record = await dbContext.DomainEventRecords.SingleAsync(r => r.Id == recordId);
            record.DispatchedAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        await RunWorkerAsync(bus, TimeSpan.FromSeconds(1), () => Task.FromResult(false));

        await bus.DidNotReceiveWithAnyArgs().Publish(default!);
    }

    [Fact]
    public async Task Worker_LeavesRecordUndispatched_WhenPublishFails()
    {
        var bus = Substitute.For<IBus>();
        bus.Publish(Arg.Any<object>()).Returns(_ => throw new InvalidOperationException("boom"));

        var recordId = await PersistDrinkCreatedEventAsync("Publish Failure");

        await RunWorkerAsync(bus, TimeSpan.FromSeconds(1), () => Task.FromResult(false));

        await using var verificationContext = fixture.CreateDbContext();
        var record = await verificationContext.DomainEventRecords.SingleAsync(r => r.Id == recordId);
        record.DispatchedAtUtc.ShouldBeNull();
    }

    [Fact]
    public async Task Worker_LeavesRecordUndispatched_WhenEventTypeIsUnknown()
    {
        var bus = Substitute.For<IBus>();
        var recordId = await PersistOutboxRecordAsync(
            "drink-catalog.unknown.v1",
            """{"drinkId":"00000000-0000-0000-0000-000000000000"}""");

        await RunWorkerAsync(bus, TimeSpan.FromSeconds(1), () => Task.FromResult(false));

        await using var verificationContext = fixture.CreateDbContext();
        var record = await verificationContext.DomainEventRecords.SingleAsync(r => r.Id == recordId);
        record.DispatchedAtUtc.ShouldBeNull();
        await bus.DidNotReceiveWithAnyArgs().Publish(default!);
    }

    [Fact]
    public async Task Worker_LeavesRecordUndispatched_WhenPayloadCannotBeDeserialized()
    {
        var bus = Substitute.For<IBus>();
        var recordId = await PersistOutboxRecordAsync(
            "drink-catalog.drink-created.v1",
            "\"oops\"");

        await RunWorkerAsync(bus, TimeSpan.FromSeconds(1), () => Task.FromResult(false));

        await using var verificationContext = fixture.CreateDbContext();
        var record = await verificationContext.DomainEventRecords.SingleAsync(r => r.Id == recordId);
        record.DispatchedAtUtc.ShouldBeNull();
        await bus.DidNotReceiveWithAnyArgs().Publish(default!);
    }

    private async Task<long> PersistDrinkCreatedEventAsync(string name)
    {
        await using var dbContext = fixture.CreateDbContext();
        var drink = Drink.Create(DrinkName.Create(name), null, ImageUrl.Create(null));
        dbContext.Drinks.Add(drink);
        await dbContext.SaveChangesAsync();

        var record = await dbContext.DomainEventRecords
            .OrderByDescending(r => r.Id)
            .FirstAsync();

        return record.Id;
    }

    private async Task<long> PersistOutboxRecordAsync(string eventType, string payload)
    {
        await using var dbContext = fixture.CreateDbContext();
        dbContext.DomainEventRecords.Add(new DomainEventRecord
        {
            AggregateId = Guid.NewGuid(),
            AggregateType = "Drink",
            EventType = eventType,
            Payload = payload,
            OccurredAtUtc = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var record = await dbContext.DomainEventRecords
            .OrderByDescending(r => r.Id)
            .FirstAsync();

        return record.Id;
    }

    private async Task RunWorkerAsync(IBus bus, TimeSpan timeout, Func<Task<bool>> completion)
    {
        using var serviceProvider = new ServiceCollection()
            .AddScoped(_ => fixture.CreateDbContext())
            .AddScoped<DrinkCatalogDbContext>(_ => fixture.CreateDbContext())
            .BuildServiceProvider();

        var worker = new OutboxWorker(
            serviceProvider,
            [new OutboxSourceDescriptor("drink-catalog", typeof(DrinkCatalogDbContext))],
            DomainEventTypeRegistry.CreateFrom(typeof(DrinkCreatedEvent).Assembly),
            bus,
            Options.Create(new OutboxWorkerOptions { BatchSize = 10, PollInterval = TimeSpan.FromMilliseconds(100) }),
            NullLogger<OutboxWorker>.Instance);

        using var cancellationTokenSource = new CancellationTokenSource(timeout);
        await worker.StartAsync(cancellationTokenSource.Token);

        try
        {
            var deadline = DateTimeOffset.UtcNow.Add(timeout);

            while (DateTimeOffset.UtcNow < deadline)
            {
                if (await completion())
                {
                    return;
                }

                await Task.Delay(100, cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Timebox expired. The caller will perform any final assertions.
        }
        finally
        {
            cancellationTokenSource.Cancel();
            await worker.StopAsync(CancellationToken.None);
        }
    }
}
