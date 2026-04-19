using AlCopilot.Recommendation.Contracts.Events;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Workflows;
using AlCopilot.Shared.Data;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        services.AddSingleton<IRecommendationChatClientStrategyFactory, RecommendationChatClientStrategyFactory>();
        services.AddSingleton<RecommendationNarrationContextProvider>();
        services.AddSingleton<AIAgent>(sp =>
        {
            var strategyFactory = sp.GetRequiredService<IRecommendationChatClientStrategyFactory>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var narrationContextProvider = sp.GetRequiredService<RecommendationNarrationContextProvider>();
            var strategy = strategyFactory.Create();
            var chatOptions = strategy.ChatOptions;
            chatOptions.Instructions =
                """
                You are an experienced bartender.
                Base your answer only on the provided customer context and deterministic recommendation candidates.
                Prefer concise, practical guidance.
                Do not invent unavailable drinks or ignore prohibited ingredients.
                """;

            return new ChatClientAgent(
                strategy.ChatClient,
                new ChatClientAgentOptions
                {
                    Name = "recommendation-narrator",
                    Description = "Turns deterministic recommendation candidates into a concise bartender-style response.",
                    ChatOptions = chatOptions,
                    ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions
                    {
                        ChatReducer = new MessageWindowChatReducer(strategy.MaxHistoryMessages),
                    }),
                    AIContextProviders =
                    [
                        narrationContextProvider,
                    ],
                },
                loggerFactory,
                sp);
        });
        services.AddScoped<IRecommendationWorkflow, RecommendationWorkflow>();
        services.AddSingleton<AgentFrameworkRecommendationNarrator>();
        services.AddSingleton<IRecommendationNarrator>(sp => sp.GetRequiredService<AgentFrameworkRecommendationNarrator>());
        services.AddOptions<RecommendationLlmOptions>()
            .Bind(configuration.GetSection(RecommendationLlmOptions.SectionName));
        services.AddOptions<RecommendationOllamaOptions>()
            .Bind(configuration.GetSection(RecommendationOllamaOptions.SectionName));

        return services;
    }
}
