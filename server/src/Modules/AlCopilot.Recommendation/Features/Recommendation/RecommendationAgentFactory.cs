using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationAgentFactory(
    IRecommendationChatClientStrategyFactory strategyFactory,
    ILoggerFactory loggerFactory,
    IServiceProvider services) : IRecommendationAgentFactory
{
    public RecommendationAgentRuntime Create(
        RecommendationAgentDefinition definition,
        string contextInstructions,
        ChatSession session)
    {
        var strategy = strategyFactory.Create();
        var historyProvider = new RecommendationChatHistoryProvider(session, strategy.MaxHistoryTurns);
        var contextProvider = new RecommendationContextProvider(contextInstructions);
        var chatOptions = strategy.ChatOptions.Clone();
        chatOptions.Instructions = definition.Instructions;
        var agent = new ChatClientAgent(
            strategy.ChatClient,
            new ChatClientAgentOptions
            {
                Name = definition.Name,
                Description = definition.Description,
                ChatOptions = chatOptions,
                ChatHistoryProvider = historyProvider,
                AIContextProviders = [contextProvider],
            },
            loggerFactory,
            services);

        return new RecommendationAgentRuntime(
            agent,
            chatOptions,
            strategy.MaxHistoryTurns);
    }
}
