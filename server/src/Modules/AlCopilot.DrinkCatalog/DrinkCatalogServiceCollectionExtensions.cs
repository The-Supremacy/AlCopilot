using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.DrinkCatalog;

public static class DrinkCatalogServiceCollectionExtensions
{
    public static IServiceCollection AddDrinkCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("drink-catalog")
            ?? throw new InvalidOperationException("Connection string 'drink-catalog' is required.");

        services.AddDbContext<DrinkCatalogDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddOutboxSource<DrinkCatalogDbContext>(
            name: "drink-catalog",
            schema: "drink_catalog",
            tableName: "domain_events");

        return services;
    }
}
