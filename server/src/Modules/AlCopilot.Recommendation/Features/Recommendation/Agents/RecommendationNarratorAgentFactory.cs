using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationNarratorAgentFactory : IRecommendationNarratorAgentFactory
{
    private readonly IRecommendationChatClientStrategyFactory strategyFactory;
    private readonly ILoggerFactory loggerFactory;
    private readonly IServiceProvider serviceProvider;
    private readonly IOptions<RecommendationObservabilityOptions> observabilityOptions;
    private readonly RecommendationRecipeLookupTool recipeLookupTool;

    public RecommendationNarratorAgentFactory(
        IRecommendationChatClientStrategyFactory strategyFactory,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        IOptions<RecommendationObservabilityOptions> observabilityOptions,
        RecommendationRecipeLookupTool recipeLookupTool)
    {
        this.strategyFactory = strategyFactory;
        this.loggerFactory = loggerFactory;
        this.serviceProvider = serviceProvider;
        this.observabilityOptions = observabilityOptions;
        this.recipeLookupTool = recipeLookupTool;
    }

    public AIAgent Create()
    {
        var strategy = strategyFactory.Create();
        var contextProvider = new RecommendationRunContextProvider(
            serviceProvider.GetRequiredService<IServiceScopeFactory>());
        var instrumentedChatClient = new ChatClientBuilder(strategy.ChatClient)
            .UseOpenTelemetry(
                loggerFactory,
                RecommendationTelemetry.SourceName,
                configure: telemetry => telemetry.EnableSensitiveData = observabilityOptions.Value.EnableSensitiveData)
            .Build();

        var chatOptions = strategy.ChatOptions;
        chatOptions.Instructions =
            """
            You are an experienced bartender.
            Base your answer only on the provided customer context and deterministic recommendation candidates.
            Prefer concise, practical guidance.
            Do not invent unavailable drinks or ignore prohibited ingredients.
            """;
        chatOptions.Tools =
        [
            AIFunctionFactory.Create(
                recipeLookupTool.LookupDrinkRecipeAsync,
                new AIFunctionFactoryOptions
                {
                    Name = "lookup_drink_recipe",
                    Description = "Look up the full recipe details for a specific known drink from the catalog when exact measurements, method, garnish, or brand detail is needed."
                }),
        ];
        chatOptions.AllowMultipleToolCalls = false;

        var agent = new ChatClientAgent(
            instrumentedChatClient,
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

        return agent.AsBuilder()
            .UseOpenTelemetry(
                RecommendationTelemetry.SourceName,
                configure: telemetry => telemetry.EnableSensitiveData = observabilityOptions.Value.EnableSensitiveData)
            .Build(serviceProvider);
    }
}
