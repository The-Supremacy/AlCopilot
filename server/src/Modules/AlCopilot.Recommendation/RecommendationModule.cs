using AlCopilot.Recommendation.Contracts.Events;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
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

        services.AddDomainEventAssembly(typeof(RecommendationSessionStartedEvent).Assembly);
        services.AddScoped<DomainEventInterceptor>();

        services.AddDbContext<RecommendationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "recommendation"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
        });

        services.AddScoped<IRecommendationUnitOfWork>(sp => sp.GetRequiredService<RecommendationDbContext>());
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IRecommendationSessionQueryService, RecommendationSessionQueryService>();
        services.AddScoped<IRecommendationCandidateBuilder, DeterministicRecommendationCandidateBuilder>();
        services.AddScoped<IRecommendationRunContextQueryService, RecommendationRunContextQueryService>();
        services.AddSingleton<IRecommendationChatClientStrategyFactory, RecommendationChatClientStrategyFactory>();
        services.AddScoped<IRecommendationNarratorAgentFactory, RecommendationNarratorAgentFactory>();
        services.AddSingleton<IRecommendationAgentSessionStore, RecommendationAgentSessionStore>();
        services.AddScoped<IRecommendationToolInvocationRecorder, RecommendationToolInvocationRecorder>();
        services.AddScoped<RecommendationRecipeLookupTool>();
        services.AddScoped<IRecommendationConversationService, RecommendationConversationService>();
        services.AddSingleton<IRecommendationEmbeddingRuntime, RecommendationEmbeddingRuntime>();
        services.AddOptions<RecommendationLlmOptions>()
            .Bind(configuration.GetSection(RecommendationLlmOptions.SectionName));
        services.AddOptions<RecommendationOllamaOptions>()
            .Bind(configuration.GetSection(RecommendationOllamaOptions.SectionName));
        services.AddOptions<RecommendationObservabilityOptions>()
            .Bind(configuration.GetSection(RecommendationObservabilityOptions.SectionName));

        return services;
    }
}
