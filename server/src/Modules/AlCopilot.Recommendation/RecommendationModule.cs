using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.Recommendation;

public static class RecommendationModule
{
    public static IServiceCollection AddRecommendationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("recommendation")
            ?? configuration.GetConnectionString("drink-catalog")
            ?? throw new InvalidOperationException(
                "Connection string 'recommendation' or fallback 'drink-catalog' is not configured.");

        services.AddDomainEventAssembly(typeof(RecommendationModule).Assembly);
        services.AddScoped<DomainEventInterceptor>();

        services.AddDbContext<RecommendationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "recommendation"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RecommendationDbContext>());
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IRecommendationSessionQueryService, RecommendationSessionQueryService>();
        services.AddScoped<IRecommendationCandidateBuilder, DeterministicRecommendationCandidateBuilder>();
        services.AddScoped<IRecommendationNarrationComposer, RecommendationNarrationComposer>();
        services.AddScoped<RecommendationKernelFactory>();
        services.AddScoped<SemanticKernelRecommendationNarrator>();
        services.AddScoped<IRecommendationNarrator>(sp => sp.GetRequiredService<SemanticKernelRecommendationNarrator>());
        services.AddScoped<RecommendationReadOnlyTools>();
        services.AddOptions<RecommendationLlmOptions>()
            .Bind(configuration.GetSection(RecommendationLlmOptions.SectionName));
        services.AddOptions<RecommendationOllamaOptions>()
            .Bind(configuration.GetSection(RecommendationOllamaOptions.SectionName));

        return services;
    }
}
