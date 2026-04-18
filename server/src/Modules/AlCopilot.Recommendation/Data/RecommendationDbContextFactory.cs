using AlCopilot.Recommendation.Contracts.Events;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.Recommendation.Data;

internal sealed class RecommendationDbContextFactory : IDesignTimeDbContextFactory<RecommendationDbContext>
{
    public RecommendationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RecommendationDbContext>();
        var services = new ServiceCollection();
        services.AddDomainEventAssembly(typeof(RecommendationSessionStartedEvent).Assembly);
        services.AddScoped<DomainEventInterceptor>();
        var serviceProvider = services.BuildServiceProvider();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=alcopilot;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "recommendation"));
        optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<DomainEventInterceptor>());

        return new RecommendationDbContext(optionsBuilder.Options);
    }
}
