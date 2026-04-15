using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.CustomerProfile.Data;

internal sealed class CustomerProfileDbContextFactory : IDesignTimeDbContextFactory<CustomerProfileDbContext>
{
    public CustomerProfileDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CustomerProfileDbContext>();
        var services = new ServiceCollection();
        services.AddDomainEventAssembly(typeof(CustomerProfileModule).Assembly);
        services.AddScoped<DomainEventInterceptor>();

        var serviceProvider = services.BuildServiceProvider();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=alcopilot;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "customer_profile"));
        optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<DomainEventInterceptor>());

        return new CustomerProfileDbContext(optionsBuilder.Options);
    }
}
