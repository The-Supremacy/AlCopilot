using AlCopilot.DrinkCatalog.Data;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public DrinkCatalogDbContext CreateDbContext()
    {
        var services = new ServiceCollection();
        services.AddScoped<DomainEventInterceptor>();
        var sp = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<DrinkCatalogDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "drink_catalog"))
            .AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>())
            .Options;

        return new DrinkCatalogDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
