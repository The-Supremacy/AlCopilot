using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.DrinkCatalog.Data;

internal sealed class DrinkCatalogDbContextFactory : IDesignTimeDbContextFactory<DrinkCatalogDbContext>
{
    public DrinkCatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DrinkCatalogDbContext>();
        var services = new ServiceCollection();
        services.AddDomainEventAssembly(typeof(DrinkCreatedEvent).Assembly);
        services.AddScoped<DomainEventInterceptor>();
        var serviceProvider = services.BuildServiceProvider();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=alcopilot;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "drink_catalog"));
        optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<DomainEventInterceptor>());
        return new DrinkCatalogDbContext(optionsBuilder.Options);
    }
}
