using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Domain;
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

        services.AddDomainEventAssembly(typeof(DrinkCreatedEvent).Assembly);

        services.AddScoped<DomainEventInterceptor>();

        services.AddDbContext<DrinkCatalogDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "drink_catalog"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DrinkCatalogDbContext>());
        services.AddScoped<IDrinkRepository, DrinkRepository>();
        services.AddScoped<IDrinkQueryService, DrinkQueryService>();
        services.AddScoped<IDrinkRecipeIntegrityValidator, DrinkRecipeIntegrityValidator>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagQueryService, TagQueryService>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        services.AddScoped<IIngredientQueryService, IngredientQueryService>();
        services.AddScoped<IImportBatchRepository, ImportBatchRepository>();
        services.AddScoped<IAuditLogEntryRepository, AuditLogEntryRepository>();
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
        services.AddScoped<AuditLogWriter>();
        services.AddScoped<ImportBatchWorkflowService>();
        services.AddScoped<IImportSourceStrategy, IbaCocktailsSnapshotImportSourceStrategy>();
        services.AddScoped<IImportSourceStrategyResolver, ImportSourceStrategyResolver>();

        return services;
    }
}
