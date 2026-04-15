using AlCopilot.Recommendation.Data;
using AlCopilot.Shared.Data;
using AlCopilot.Testing.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.Recommendation.Tests.Integration;

public sealed class PostgresFixture : PostgreSqlContainerFixture, IAsyncLifetime
{
    public RecommendationDbContext CreateDbContext()
    {
        var services = new ServiceCollection();
        services.AddDomainEventAssembly(typeof(AlCopilot.Recommendation.RecommendationModule).Assembly);
        services.AddScoped<DomainEventInterceptor>();
        var serviceProvider = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<RecommendationDbContext>()
            .UseNpgsql(ConnectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "recommendation"))
            .AddInterceptors(serviceProvider.GetRequiredService<DomainEventInterceptor>())
            .Options;

        return new RecommendationDbContext(options);
    }

    protected override async Task InitializeDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    Task IAsyncLifetime.InitializeAsync() => InitializeContainerAsync();

    Task IAsyncLifetime.DisposeAsync() => DisposeContainerAsync();
}
