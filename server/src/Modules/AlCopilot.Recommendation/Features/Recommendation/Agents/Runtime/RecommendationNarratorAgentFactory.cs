using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationNarratorAgentFactory : IRecommendationNarratorAgentFactory
{
    private readonly IRecommendationChatClientStrategyFactory strategyFactory;
    private readonly ILoggerFactory loggerFactory;
    private readonly IOptions<RecommendationObservabilityOptions> observabilityOptions;
    private readonly IRecommendationRunInputsQueryService runInputsQueryService;
    private readonly IRecommendationSemanticSearchService semanticSearchService;
    private readonly IRecommendationRequestIntentResolver requestIntentResolver;
    private readonly IRecommendationCandidateBuilder candidateBuilder;
    private readonly IRecommendationRunContextBuilder runContextBuilder;
    private readonly IRecommendationExecutionTraceRecorder executionTraceRecorder;
    private readonly IServiceProvider serviceProvider;
    private readonly RecommendationDrinkSearchTool drinkSearchTool;
    private readonly RecommendationRecipeLookupTool recipeLookupTool;
    private readonly RecommendationIngredientLookupTool ingredientLookupTool;

    public RecommendationNarratorAgentFactory(
        IRecommendationChatClientStrategyFactory strategyFactory,
        ILoggerFactory loggerFactory,
        IOptions<RecommendationObservabilityOptions> observabilityOptions,
        IRecommendationRunInputsQueryService runInputsQueryService,
        IRecommendationSemanticSearchService semanticSearchService,
        IRecommendationRequestIntentResolver requestIntentResolver,
        IRecommendationCandidateBuilder candidateBuilder,
        IRecommendationRunContextBuilder runContextBuilder,
        IRecommendationExecutionTraceRecorder executionTraceRecorder,
        IServiceProvider serviceProvider,
        RecommendationDrinkSearchTool drinkSearchTool,
        RecommendationRecipeLookupTool recipeLookupTool,
        RecommendationIngredientLookupTool ingredientLookupTool)
    {
        this.strategyFactory = strategyFactory;
        this.loggerFactory = loggerFactory;
        this.observabilityOptions = observabilityOptions;
        this.runInputsQueryService = runInputsQueryService;
        this.semanticSearchService = semanticSearchService;
        this.requestIntentResolver = requestIntentResolver;
        this.candidateBuilder = candidateBuilder;
        this.runContextBuilder = runContextBuilder;
        this.executionTraceRecorder = executionTraceRecorder;
        this.serviceProvider = serviceProvider;
        this.drinkSearchTool = drinkSearchTool;
        this.recipeLookupTool = recipeLookupTool;
        this.ingredientLookupTool = ingredientLookupTool;
    }

    public AIAgent Create(ChatSession session)
    {
        var strategy = strategyFactory.Create();
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
            Base your answer on the provided customer context, deterministic recommendation candidates, and tool results.
            Follow the resolved request intent from the run context.
            Prefer deterministic recommendation candidates when they satisfy the request.
            If you need to resolve a drink name before looking up its recipe, call the drink search tool.
            If the request is ingredient-led, or the deterministic candidates do not cover the request well enough, call the ingredient lookup tool before answering.
            If exact measurements, method, garnish, or brand details are needed for a specific known drink, call the recipe lookup tool after you know which drink to inspect.
            Prefer concise, practical guidance.
            Do not invent unavailable drinks or ignore prohibited ingredients.
            """;
        chatOptions.Tools =
        [
            AIFunctionFactory.Create(
                drinkSearchTool.SearchDrinksAsync,
                new AIFunctionFactoryOptions
                {
                    Name = "search_drinks",
                    Description = "Search the catalog by drink name or partial drink name before looking up a drink recipe."
                }),
            AIFunctionFactory.Create(
                ingredientLookupTool.LookupDrinksByIngredientAsync,
                new AIFunctionFactoryOptions
                {
                    Name = "lookup_drinks_by_ingredient",
                    Description = "Find catalog drinks that contain a requested ingredient when the customer asks for something with tequila, rum, gin, citrus, or another ingredient."
                }),
            AIFunctionFactory.Create(
                recipeLookupTool.LookupDrinkRecipeAsync,
                new AIFunctionFactoryOptions
                {
                    Name = "lookup_drink_recipe",
                    Description = "Look up the full recipe details for a specific known drink from the catalog when exact measurements, method, garnish, or brand detail is needed."
                }),
        ];
        chatOptions.AllowMultipleToolCalls = true;

        var agent = new ChatClientAgent(
            instrumentedChatClient,
            new ChatClientAgentOptions
            {
                Name = "recommendation-narrator",
                Description = "Turns deterministic recommendation candidates into a concise bartender-style response.",
                ChatOptions = chatOptions,
                ChatHistoryProvider = new RecommendationChatHistoryProvider(session),
                AIContextProviders =
                [
                    new RecommendationInvocationContextProvider(session.Id),
                    new RecommendationCatalogInputsProvider(runInputsQueryService),
                    new RecommendationSemanticSearchProvider(semanticSearchService),
                    new RecommendationIntentResolutionProvider(requestIntentResolver),
                    new RecommendationNarrationContextProvider(
                        candidateBuilder,
                        runContextBuilder,
                        executionTraceRecorder),
                ],
            },
            loggerFactory,
            serviceProvider);

        var builtAgent = agent.AsBuilder()
            .UseOpenTelemetry(
                RecommendationTelemetry.SourceName,
                configure: telemetry => telemetry.EnableSensitiveData = observabilityOptions.Value.EnableSensitiveData)
            .Build(serviceProvider);

        return builtAgent;
    }
}
