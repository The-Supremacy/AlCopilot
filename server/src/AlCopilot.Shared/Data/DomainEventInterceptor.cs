using System.Text.Json;
using AlCopilot.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AlCopilot.Shared.Data;

public sealed class DomainEventInterceptor(
    IDomainEventDispatcher domainEventDispatcher,
    DomainEventTypeRegistry eventTypeRegistry) : SaveChangesInterceptor
{
    private const int MaxDispatchDepth = 5;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var dispatchDepth = 0;

        while (true)
        {
            if (dispatchDepth >= MaxDispatchDepth)
                throw new InvalidOperationException(
                    $"Domain event dispatch loop exceeded maximum depth of {MaxDispatchDepth}.");

            var hadEvents = await DispatchCurrentBatchAsync(context, cancellationToken);
            if (!hadEvents)
                return;

            dispatchDepth++;
        }
    }

    private async Task<bool> DispatchCurrentBatchAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        var aggregatesWithEvents = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => (AggregateTypeName: e.Entity.GetType().Name, Events: e.Entity.DomainEvents.ToList(), Entity: e.Entity))
            .ToList();

        if (aggregatesWithEvents.Count == 0)
            return false;

        foreach (var (_, _, entity) in aggregatesWithEvents)
            entity.ClearDomainEvents();

        var domainEventSet = context.Set<DomainEventRecord>();
        var allEvents = new List<IDomainEvent>();

        foreach (var (aggregateTypeName, events, _) in aggregatesWithEvents)
        {
            foreach (var domainEvent in events)
            {
                allEvents.Add(domainEvent);
                domainEventSet.Add(new DomainEventRecord
                {
                    AggregateId = domainEvent.AggregateId,
                    AggregateType = aggregateTypeName,
                    EventType = eventTypeRegistry.GetName(domainEvent.GetType()),
                    Payload = JsonSerializer.Serialize((object)domainEvent),
                    OccurredAtUtc = domainEvent.OccurredAtUtc,
                });
            }
        }

        foreach (var domainEvent in allEvents)
            await domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);

        return true;
    }
}
