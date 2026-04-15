using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using AlCopilot.Shared.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.Shared.Data;

public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, IDomainEvent, CancellationToken, Task>> Dispatchers = new();

    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var dispatcher = Dispatchers.GetOrAdd(domainEvent.GetType(), CreateDispatcher);
        return dispatcher(serviceProvider, domainEvent, cancellationToken);
    }

    private static Func<IServiceProvider, IDomainEvent, CancellationToken, Task> CreateDispatcher(Type eventType)
    {
        var dispatchMethod = typeof(DomainEventDispatcher)
            .GetMethod(nameof(DispatchTypedAsync), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(eventType);

        var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        var domainEventParameter = Expression.Parameter(typeof(IDomainEvent), "domainEvent");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var call = Expression.Call(
            dispatchMethod,
            serviceProviderParameter,
            Expression.Convert(domainEventParameter, eventType),
            cancellationTokenParameter);

        return Expression.Lambda<Func<IServiceProvider, IDomainEvent, CancellationToken, Task>>(
            call,
            serviceProviderParameter,
            domainEventParameter,
            cancellationTokenParameter).Compile();
    }

    private static async Task DispatchTypedAsync<TEvent>(
        IServiceProvider serviceProvider,
        TEvent domainEvent,
        CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        var handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(domainEvent, cancellationToken);
        }
    }
}
