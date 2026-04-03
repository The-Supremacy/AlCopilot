using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxSource<TDbContext>(
        this IServiceCollection services,
        string name,
        string schema,
        string tableName)
        where TDbContext : DbContext
    {
        return services.AddOutboxSource(name, typeof(TDbContext), schema, tableName);
    }

    public static IServiceCollection AddOutboxSource(
        this IServiceCollection services,
        string name,
        Type dbContextType,
        string schema,
        string tableName)
    {
        var descriptor = new OutboxSourceDescriptor(name, dbContextType, schema, tableName);
        descriptor.Validate();

        var alreadyRegistered = services
            .Where(service => service.ServiceType == typeof(OutboxSourceDescriptor))
            .Select(service => service.ImplementationInstance as OutboxSourceDescriptor)
            .Any(existing => existing == descriptor);

        if (!alreadyRegistered)
        {
            services.AddSingleton(descriptor);
        }

        return services;
    }
}
