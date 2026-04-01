using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Data.Repositories;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.DrinkCatalog;

public static class DrinkCatalogModule
{
    public static IServiceCollection AddDrinkCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("drink-catalog")
            ?? throw new InvalidOperationException("Connection string 'drink-catalog' is not configured.");

        services.AddScoped<DomainEventInterceptor>();

        services.AddDbContext<DrinkCatalogDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "drink_catalog"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DrinkCatalogDbContext>());
        services.AddScoped<IDrinkRepository, DrinkRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        services.AddScoped<IIngredientCategoryRepository, IngredientCategoryRepository>();

        return services;
    }
}
