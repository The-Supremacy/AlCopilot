using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Data;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Compaction;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationNarratorAgentFactory : IRecommendationNarratorAgentFactory
{
    private const string NarratorInstructions =
        """
        You are an experienced bartender.
        Answer from the provided run context, chat history, and tool results.
        Follow the resolved request intent in the run context.
        Use chat history to resolve follow-up references such as "that", "it", or "the first one".

        Recommendation policy:
        Choose from the grounded candidates in the run context when they satisfy the request.
        Recommend one best-fit option from the highest-priority current group; use bartender judgment for taste and occasion fit.
        Do not name lower-priority or restock alternatives unless the user asks for alternatives or another option.
        Explain conflicts with prohibited ingredients and do not recommend drinks containing them.
        Prefer drinks without disliked ingredients when a suitable option exists.
        When the current request excludes an ingredient, explicitly acknowledge that ingredient by name and recommend a candidate that avoids it.
        Never name a drink that violates the current prohibited or disliked ingredient constraint unless the user explicitly asked about that drink.
        Do not invent unavailable catalog drinks.

        Tool policy:
        Use the tool descriptions to choose the required tool.
        For ordinary recommendation requests, answer from deterministic candidates and do not call recipe lookup unless the user asks for recipe details, exact measurements, method, garnish, brand details, or how to make a specific drink.
        If a recipe or drink-details request names a drink that is not resolved in the run context and not found by drink search, say it is unavailable in the catalog instead of calling recipe lookup or inventing the recipe.
        Never call recipe lookup for a drink name that is absent from the run context and absent from a prior drink search result.
        Do not answer recipe, measurement, method, garnish, or brand-detail requests from memory or deterministic candidates alone; use recipe lookup.
        Do not use recipe lookup only to summarize an ordinary recommendation when the run context already contains enough detail.
        When a recipe lookup is used, include the returned recipe ingredients in the answer.

        Prefer concise, practical guidance.
        """;

    private readonly IRecommendationChatClientStrategyFactory strategyFactory;
    private readonly ILoggerFactory loggerFactory;
    private readonly RecommendationDbContext dbContext;
    private readonly IOptions<RecommendationObservabilityOptions> observabilityOptions;
    private readonly IOptions<RecommendationCompactionOptions> compactionOptions;
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
        RecommendationDbContext dbContext,
        IOptions<RecommendationObservabilityOptions> observabilityOptions,
        IOptions<RecommendationCompactionOptions> compactionOptions,
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
        this.dbContext = dbContext;
        this.observabilityOptions = observabilityOptions;
        this.compactionOptions = compactionOptions;
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

    public RecommendationNarratorAgentRuntime Create(ChatSession session, AgentRun agentRun)
    {
        RecommendationRunContext? runContext = null;
        var strategy = strategyFactory.Create();
        var chatClientBuilder = new ChatClientBuilder(strategy.ChatClient);
        ConfigureCompaction(chatClientBuilder);
        var instrumentedChatClient = chatClientBuilder
            .UseOpenTelemetry(
                loggerFactory,
                RecommendationTelemetry.SourceName,
                configure: telemetry => telemetry.EnableSensitiveData = observabilityOptions.Value.EnableSensitiveData)
            .Build();
        var chatOptions = strategy.ChatOptions;
        chatOptions.Instructions = NarratorInstructions;
        chatOptions.Tools =
        [
            AIFunctionFactory.Create(
                drinkSearchTool.SearchDrinksAsync,
                new AIFunctionFactoryOptions
                {
                    Name = "search_drinks",
                    Description = "Search catalog drink names. Use when the user explicitly asks to search/find matching drink names, gives a partial or uncertain drink name, or a drink name must be resolved before recipe lookup. Do not use for ingredient-only searches."
                }),
            AIFunctionFactory.Create(
                ingredientLookupTool.LookupDrinksByIngredientAsync,
                new AIFunctionFactoryOptions
                {
                    Name = "lookup_drinks_by_ingredient",
                    Description = "Find catalog drinks containing a requested ingredient. Use for questions like 'which drinks use ginger beer?' or recommendation requests centered on an ingredient when deterministic candidates are insufficient. Do not use for exact recipe details."
                }),
            AIFunctionFactory.Create(
                recipeLookupTool.LookupDrinkRecipeAsync,
                new AIFunctionFactoryOptions
                {
                    Name = "lookup_drink_recipe",
                    Description = "Look up full recipe details for one specific known drink that appears in the run context or a prior search_drinks result. Use for 'how do I make it?', exact ingredients, measurements, method, garnish, or brand-detail requests after the drink is known. Do not use for ordinary recommendation lists, catalog search, ingredient-list queries, or unavailable drinks."
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
                ChatHistoryProvider = new RecommendationChatHistoryProvider(
                    dbContext,
                    session,
                    agentRun.Id),
                AIContextProviders =
                [
                    new RecommendationRunContextProvider(
                        runInputsQueryService,
                        semanticSearchService,
                        requestIntentResolver,
                        candidateBuilder,
                        runContextBuilder,
                        executionTraceRecorder,
                        value => runContext = value),
                ],
            },
            loggerFactory,
            serviceProvider);

        var builtAgent = agent.AsBuilder()
            .UseOpenTelemetry(
                RecommendationTelemetry.SourceName,
                configure: telemetry => telemetry.EnableSensitiveData = observabilityOptions.Value.EnableSensitiveData)
            .Build(serviceProvider);

        return new RecommendationNarratorAgentRuntime(
            builtAgent,
            () => runContext,
            strategy.Provider,
            strategy.Model);
    }

    private void ConfigureCompaction(ChatClientBuilder builder)
    {
        var options = compactionOptions.Value;
        if (!options.Enabled)
        {
            return;
        }

#pragma warning disable MAAI001
        builder.UseAIContextProviders(
            new CompactionProvider(
                new ToolResultCompactionStrategy(
                    CompactionTriggers.All(
                        CompactionTriggers.HasToolCalls(),
                        CompactionTriggers.Any(
                            CompactionTriggers.GroupsExceed(options.ToolResultGroupsThreshold),
                            CompactionTriggers.TokensExceed(options.ToolResultTokenThreshold))),
                    options.MinimumPreservedGroups),
                "recommendation-tool-result-compaction",
                loggerFactory));
#pragma warning restore MAAI001
    }
}
