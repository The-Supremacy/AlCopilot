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
        services.AddScoped<IAgentRunRepository, AgentRunRepository>();
        services.AddScoped<IAgentMessageRepository, AgentMessageRepository>();
        services.AddScoped<IAgentMessageDiagnosticRepository, AgentMessageDiagnosticRepository>();
        services.AddScoped<IRecommendationTurnOutputRepository, RecommendationTurnOutputRepository>();
        services.AddScoped<IRecommendationSessionQueryService, RecommendationSessionQueryService>();
        services.AddScoped<IRecommendationCatalogFuzzyLookupService, RecommendationCatalogFuzzyLookupService>();
        services.AddScoped<IRecommendationCandidateBuilder, DeterministicRecommendationCandidateBuilder>();
        services.AddScoped<IRecommendationRunInputsQueryService, RecommendationRunInputsQueryService>();
        services.AddScoped<IRecommendationSemanticIndexingService, RecommendationSemanticIndexingService>();
        services.AddScoped<IRecommendationSemanticSearchService, RecommendationSemanticSearchService>();
        services.AddScoped<IRecommendationRequestIntentResolver, RecommendationRequestIntentResolver>();
        services.AddScoped<IRecommendationRunContextBuilder, RecommendationRunContextBuilder>();
        services.AddSingleton<IRecommendationChatClientStrategyFactory, RecommendationChatClientStrategyFactory>();
        services.AddScoped<IRecommendationNarratorAgentFactory, RecommendationNarratorAgentFactory>();
        services.AddSingleton<IRecommendationAgentSessionStore, RecommendationAgentSessionStore>();
        services.AddScoped<IRecommendationExecutionTraceRecorder, RecommendationExecutionTraceRecorder>();
        services.AddScoped<IRecommendationAgentRunDiagnosticsRecorder, RecommendationAgentRunDiagnosticsRecorder>();
        services.AddScoped<RecommendationDrinkSearchTool>();
        services.AddScoped<RecommendationIngredientLookupTool>();
        services.AddScoped<RecommendationRecipeLookupTool>();
        services.AddScoped<IRecommendationConversationService, RecommendationConversationService>();
        services.AddSingleton<IRecommendationVectorStore, RecommendationQdrantVectorStore>();
        services.AddSingleton<IRecommendationEmbeddingClientFactory, RecommendationEmbeddingClientFactory>();
        services.AddOptions<RecommendationLlmOptions>()
            .Bind(configuration.GetSection(RecommendationLlmOptions.SectionName));
        services.AddOptions<RecommendationOllamaOptions>()
            .Bind(configuration.GetSection(RecommendationOllamaOptions.SectionName));
        services.AddOptions<RecommendationSemanticOptions>()
            .Bind(configuration.GetSection(RecommendationSemanticOptions.SectionName));
        services.AddOptions<RecommendationCompactionOptions>()
            .Bind(configuration.GetSection(RecommendationCompactionOptions.SectionName));
        services.AddOptions<RecommendationObservabilityOptions>()
            .Bind(configuration.GetSection(RecommendationObservabilityOptions.SectionName));

        return services;
    }
}
