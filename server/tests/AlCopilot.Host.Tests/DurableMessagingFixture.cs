using AlCopilot.DrinkCatalog;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace AlCopilot.Host.Tests;

public sealed class DurableMessagingFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await MigrateDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public DrinkCatalogDbContext CreateDbContext()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("ConnectionStrings:drink-catalog", PostgresConnectionString)
            ])
            .Build();

        var services = new ServiceCollection();
        services.AddDrinkCatalogModule(configuration);

        return services.BuildServiceProvider().GetRequiredService<DrinkCatalogDbContext>();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM drink_catalog.\"RecipeEntries\"; " +
            "DELETE FROM drink_catalog.\"DrinkTag\"; " +
            "DELETE FROM drink_catalog.\"Drinks\"; " +
            "DELETE FROM drink_catalog.\"Tags\"; " +
            "DELETE FROM drink_catalog.\"Ingredients\"; " +
            "DELETE FROM drink_catalog.\"IngredientCategories\"; " +
            "DELETE FROM drink_catalog.domain_events;");
    }

    private async Task MigrateDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
    }
}

[CollectionDefinition("DurableMessaging")]
public sealed class DurableMessagingCollection : ICollectionFixture<DurableMessagingFixture>;
