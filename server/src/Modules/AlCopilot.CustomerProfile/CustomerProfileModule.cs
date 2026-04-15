using AlCopilot.CustomerProfile.Data;
using AlCopilot.CustomerProfile.Features.Profile;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.CustomerProfile;

public static class CustomerProfileModule
{
    public static IServiceCollection AddCustomerProfileModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("customer-profile")
            ?? configuration.GetConnectionString("drink-catalog")
            ?? throw new InvalidOperationException(
                "Connection string 'customer-profile' or fallback 'drink-catalog' is not configured.");

        services.AddDomainEventAssembly(typeof(CustomerProfileModule).Assembly);
        services.AddScoped<DomainEventInterceptor>();

        services.AddDbContext<CustomerProfileDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "customer_profile"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CustomerProfileDbContext>());
        services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
        services.AddScoped<ICustomerProfileQueryService, CustomerProfileQueryService>();

        return services;
    }
}
