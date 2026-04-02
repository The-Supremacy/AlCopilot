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
        var registry = DomainEventTypeRegistry.CreateFrom(typeof(DrinkCatalogModule).Assembly);
        var services = new ServiceCollection().BuildServiceProvider();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=alcopilot;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "drink_catalog"));
        optionsBuilder.AddInterceptors(new DomainEventInterceptor(services, registry));
        return new DrinkCatalogDbContext(optionsBuilder.Options);
    }
}
