using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.Testing.Shared;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

public sealed class PostgresFixture : PostgreSqlContainerFixture, IAsyncLifetime
{
    public DrinkCatalogDbContext CreateDbContext()
    {
        // Intentionally empty service provider — no domain event handlers registered.
        // The interceptor persists DomainEventRecords but dispatches to zero handlers.
        var services = new ServiceCollection();
        services.AddDomainEventAssembly(typeof(DrinkCreatedEvent).Assembly);
        services.AddScoped<DomainEventInterceptor>();
        var sp = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<DrinkCatalogDbContext>()
            .UseNpgsql(ConnectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "drink_catalog"))
            .AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>())
            .Options;

        return new DrinkCatalogDbContext(options);
    }

    protected override async Task InitializeDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    Task IAsyncLifetime.InitializeAsync() => InitializeContainerAsync();

    Task IAsyncLifetime.DisposeAsync() => DisposeContainerAsync();
}
