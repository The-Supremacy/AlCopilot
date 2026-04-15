using System.Reflection;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Domain;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DomainEventRegistryServiceCollectionExtensions
{
    public static IServiceCollection AddDomainEventAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton(new DomainEventAssemblyMarker(assembly));

        // (Re-)register the registry factory. DI resolves the last registration,
        // which will collect all markers added by every module.
        services.AddSingleton(sp =>
            DomainEventTypeRegistry.CreateFrom(
                sp.GetServices<DomainEventAssemblyMarker>()
                    .Select(m => m.Assembly)
                    .ToArray()));

        return services;
    }
}

public sealed class DomainEventAssemblyMarker(Assembly assembly)
{
    public Assembly Assembly { get; } = assembly;
}
