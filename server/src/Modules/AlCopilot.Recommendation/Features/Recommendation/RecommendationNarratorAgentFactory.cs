using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationNarratorAgentFactory : IRecommendationNarratorAgentFactory
{
    private readonly IRecommendationChatClientStrategyFactory strategyFactory;
    private readonly ILoggerFactory loggerFactory;
    private readonly IServiceProvider serviceProvider;

    public RecommendationNarratorAgentFactory(
        IRecommendationChatClientStrategyFactory strategyFactory,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider)
    {
        this.strategyFactory = strategyFactory;
        this.loggerFactory = loggerFactory;
        this.serviceProvider = serviceProvider;
    }

    public AIAgent Create()
    {
        var strategy = strategyFactory.Create();
        var contextProvider = new RecommendationNarrationContextProvider(
            serviceProvider.GetRequiredService<IServiceScopeFactory>());

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
                ChatHistoryProvider = new NoOpChatHistoryProvider(),
                AIContextProviders =
                [
                    contextProvider,
                ],
            },
            loggerFactory,
            serviceProvider);
    }
}
