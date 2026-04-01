using System.Text.Json;
using AlCopilot.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.Shared.Data;

public sealed class DomainEventInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
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
        for (var depth = 0; depth < MaxDispatchDepth; depth++)
        {
            var aggregates = context.ChangeTracker
                .Entries<IAggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Count > 0)
                .Select(e => e.Entity)
                .ToList();

            if (aggregates.Count == 0)
                return;

            var events = aggregates
                .SelectMany(a => a.DomainEvents)
                .ToList();

            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            var domainEventSet = context.Set<DomainEventRecord>();
            foreach (var domainEvent in events)
            {
                domainEventSet.Add(new DomainEventRecord
                {
                    AggregateId = domainEvent.AggregateId,
                    AggregateType = domainEvent.GetType().Name.Replace("Event", string.Empty),
                    EventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize((object)domainEvent),
                    OccurredAtUtc = domainEvent.OccurredAtUtc,
                    IsPublished = false
                });
            }

            foreach (var domainEvent in events)
            {
                await DispatchToHandlersAsync(domainEvent, cancellationToken);
            }
        }

        var remaining = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Any(e => e.Entity.DomainEvents.Count > 0);

        if (remaining)
        {
            throw new InvalidOperationException(
                $"Domain event dispatch loop exceeded maximum depth of {MaxDispatchDepth}.");
        }
    }

    private async Task DispatchToHandlersAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is null) continue;

            var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
            if (method is not null)
            {
                var task = (Task)method.Invoke(handler, [domainEvent, cancellationToken])!;
                await task;
            }
        }
    }
}
