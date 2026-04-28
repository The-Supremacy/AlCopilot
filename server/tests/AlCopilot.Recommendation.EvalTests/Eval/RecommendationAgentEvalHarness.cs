using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Mediator;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text.Json;

namespace AlCopilot.Recommendation.EvalTests.Eval;

internal sealed class RecommendationAgentEvalHarness
{
    private RecommendationAgentEvalHarness()
    {
    }

    public static RecommendationAgentEvalHarness Create(RecommendationAgentEvalCorpus corpus)
    {
        ArgumentNullException.ThrowIfNull(corpus);

        return new RecommendationAgentEvalHarness();
    }

    public async Task<RecommendationAgentEvalResult> RunAsync(
        RecommendationAgentEvalCase evalCase,
        int repetitionNumber,
        CancellationToken cancellationToken = default)
    {
        var runtime = CreateRuntime(evalCase.Profile);
        return await SendMessageAsync(
            runtime.Service,
            runtime.DbContext,
            $"maf-eval-{evalCase.Name}-{repetitionNumber}",
            null,
            evalCase.Prompt,
            evalCase.Name,
            repetitionNumber,
            cancellationToken);
    }

    public async Task<AgentEvaluationResults> EvaluateAsync(
        RecommendationAgentEvalCase evalCase,
        IAgentEvaluator evaluator,
        string evalName,
        int repetitionNumber,
        CancellationToken cancellationToken = default)
    {
        var runtime = CreateRuntime(evalCase.Profile);
        var session = ChatSession.Create(
            $"maf-eval-{evalCase.Name}-{repetitionNumber}",
            evalCase.Prompt);
        var agentRun = AgentRun.Start(session.Id);
        runtime.DbContext.ChatSessions.Add(session);
        runtime.DbContext.AgentRuns.Add(agentRun);
        await runtime.DbContext.SaveChangesAsync(cancellationToken);

        var agentRuntime = runtime.AgentFactory.Create(session, agentRun);
        return await agentRuntime.Agent.EvaluateAsync(
            [evalCase.Prompt],
            evaluator,
            evalName,
            expectedOutput: [string.Join(Environment.NewLine, evalCase.ExpectedResponseFragments)],
            expectedToolCalls:
            [
                RecommendationAgentLocalEvaluator.CreateExpectedToolCalls(evalCase.ExpectedToolNames),
            ],
            numRepetitions: 1,
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<RecommendationAgentEvalResult>> RunSessionAsync(
        RecommendationAgentEvalSessionCase evalCase,
        CancellationToken cancellationToken = default)
    {
        var runtime = CreateRuntime(evalCase.Profile);
        var results = new List<RecommendationAgentEvalResult>();
        var customerId = $"maf-eval-session-{evalCase.Name}";
        Guid? sessionId = null;

        for (var turnNumber = 1; turnNumber <= evalCase.Turns.Count; turnNumber++)
        {
            var turn = evalCase.Turns[turnNumber - 1];
            var result = await SendMessageAsync(
                runtime.Service,
                runtime.DbContext,
                customerId,
                sessionId,
                turn.Prompt,
                evalCase.Name,
                turnNumber,
                cancellationToken);

            sessionId = result.SessionId;
            results.Add(result);
        }

        return results;
    }

    private static async Task<RecommendationAgentEvalResult> SendMessageAsync(
        RecommendationConversationService service,
        RecommendationDbContext dbContext,
        string customerId,
        Guid? sessionId,
        string prompt,
        string caseName,
        int turnNumber,
        CancellationToken cancellationToken)
    {
        var messageResult = await service.SendMessageAsync(
            customerId,
            sessionId,
            prompt,
            cancellationToken);
        var session = await new RecommendationSessionQueryService(dbContext).GetSessionAsync(
                customerId,
                messageResult.SessionId,
                cancellationToken)
            ?? throw new InvalidOperationException("Recommendation eval session could not be reloaded.");
        var assistantTurn = session.Turns.Last(turn => string.Equals(turn.Role, "assistant", StringComparison.Ordinal));

        return new RecommendationAgentEvalResult(
            caseName,
            turnNumber,
            session.SessionId,
            assistantTurn.Content,
            GetToolNames(dbContext, session.SessionId));
    }

    private RecommendationAgentEvalRuntime CreateRuntime(RecommendationAgentEvalProfile evalProfile)
    {
        var catalog = RecommendationAgentEvalSeedCatalog.Create();
        var profile = BuildProfile(evalProfile, catalog);
        var mediator = Substitute.For<IMediator>();

        mediator.Send(Arg.Any<GetCustomerProfileQuery>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        mediator.Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns(catalog.Drinks.ToList());
        mediator.Send(Arg.Any<GetDrinkByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var query = (GetDrinkByIdQuery)call[0]!;
                return catalog.Drinks.FirstOrDefault(drink => drink.Id == query.DrinkId);
            });

        var fuzzyLookupService = new EvalCatalogFuzzyLookupService(catalog);
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var runInputsQueryService = new RecommendationRunInputsQueryService(mediator);
        var requestIntentResolver = new RecommendationRequestIntentResolver(
            fuzzyLookupService);
        var strategyFactory = new RecommendationChatClientStrategyFactory(
            Options.Create(BuildLlmOptions()),
            Options.Create(BuildOllamaOptions()));
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var dbContext = CreateEvalDbContext();
        var drinkSearchTool = new RecommendationDrinkSearchTool(
            mediator,
            fuzzyLookupService,
            executionTraceRecorder);
        var ingredientLookupTool = new RecommendationIngredientLookupTool(
            mediator,
            fuzzyLookupService,
            executionTraceRecorder);
        var recipeLookupTool = new RecommendationRecipeLookupTool(
            mediator,
            executionTraceRecorder);
        var observabilityOptions = new RecommendationObservabilityOptions
        {
            PersistExecutionTraceInDevelopment = true,
        };
        var agentFactory = new RecommendationNarratorAgentFactory(
            strategyFactory,
            loggerFactory,
            dbContext,
            Options.Create(observabilityOptions),
            Options.Create(new RecommendationCompactionOptions()),
            runInputsQueryService,
            new EvalSemanticSearchService(),
            requestIntentResolver,
            new DeterministicRecommendationCandidateBuilder(),
            new RecommendationRunContextBuilder(),
            executionTraceRecorder,
            serviceProvider,
            drinkSearchTool,
            recipeLookupTool,
            ingredientLookupTool);
        var repository = new ChatSessionRepository(dbContext);
        var service = new RecommendationConversationService(
            repository,
            agentFactory,
            new RecommendationAgentSessionStore(),
            new RecommendationAgentRunDiagnosticsRecorder(
                executionTraceRecorder,
                new AgentMessageDiagnosticRepository(dbContext),
                new EvalHostEnvironment
                {
                    EnvironmentName = Environments.Development,
                },
                Options.Create(observabilityOptions)),
            new AgentRunRepository(dbContext),
            new RecommendationTurnOutputRepository(dbContext),
            dbContext,
            loggerFactory.CreateLogger<RecommendationConversationService>());

        return new RecommendationAgentEvalRuntime(service, dbContext, agentFactory);
    }

    private static RecommendationDbContext CreateEvalDbContext()
    {
        var options = new DbContextOptionsBuilder<RecommendationDbContext>()
            .UseInMemoryDatabase($"recommendation-eval-{Guid.NewGuid():N}")
            .Options;

        var dbContext = new RecommendationDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    private static IReadOnlyCollection<string> GetToolNames(RecommendationDbContext dbContext, Guid sessionId)
    {
        var latestRunId = dbContext.AgentRuns
            .AsNoTracking()
            .Where(run => run.ChatSessionId == sessionId)
            .OrderByDescending(run => run.StartedAtUtc)
            .Select(run => (Guid?)run.Id)
            .FirstOrDefault();

        if (latestRunId is null)
        {
            return [];
        }

        var persistedToolNames = dbContext.AgentMessages
            .AsNoTracking()
            .Where(message => message.AgentRunId == latestRunId.Value && message.Kind == "tool-call")
            .OrderBy(message => message.Sequence)
            .AsEnumerable()
            .SelectMany(message => ExtractToolNames(message.RawMessageJson))
            .ToList();
        if (persistedToolNames.Count > 0)
        {
            return persistedToolNames;
        }

        return dbContext.AgentMessageDiagnostics
            .AsNoTracking()
            .Where(diagnostic => diagnostic.AgentRunId == latestRunId.Value && diagnostic.Kind == "trace")
            .OrderBy(diagnostic => diagnostic.CreatedAtUtc)
            .Select(diagnostic => diagnostic.Name)
            .AsEnumerable()
            .Select(TryExtractToolName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList();
    }

    private static IEnumerable<string> ExtractToolNames(string rawMessageJson)
    {
        var message = JsonSerializer.Deserialize<ChatMessage>(rawMessageJson, AIJsonUtilities.DefaultOptions);
        return message?.Contents
            .OfType<FunctionCallContent>()
            .Select(content => content.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            ?? [];
    }

    private static string? TryExtractToolName(string name)
    {
        const string prefix = "tool.";
        return name.StartsWith(prefix, StringComparison.Ordinal)
            ? name[prefix.Length..]
            : null;
    }

    private CustomerProfileDto BuildProfile(
        RecommendationAgentEvalProfile profile,
        RecommendationAgentEvalSeedCatalog catalog)
    {
        return new CustomerProfileDto(
            [],
            ResolveIngredientIds(profile.DislikedIngredientNames, catalog),
            ResolveIngredientIds(profile.ProhibitedIngredientNames, catalog),
            ResolveIngredientIds(profile.OwnedIngredientNames, catalog));
    }

    private static List<Guid> ResolveIngredientIds(
        IReadOnlyCollection<string> ingredientNames,
        RecommendationAgentEvalSeedCatalog catalog)
    {
        return ingredientNames
            .Select(name => catalog.FindIngredientId(name)
                ?? throw new InvalidOperationException($"Eval seed catalog does not define ingredient '{name}'."))
            .Distinct()
            .ToList();
    }

    private static RecommendationLlmOptions BuildLlmOptions()
    {
        var temperature = TryGetFloatEnvironmentVariable("ALCOPILOT_RECOMMENDATION_AGENT_EVAL_TEMPERATURE");

        return new RecommendationLlmOptions
        {
            Provider = RecommendationLlmOptions.OllamaProvider,
            Sampling = new RecommendationSamplingOptions
            {
                Temperature = temperature ?? 0.1f,
                TopP = 0.9f,
            },
        };
    }

    private static RecommendationOllamaOptions BuildOllamaOptions()
    {
        var devSettings = ReadDevOllamaSettings();

        return new RecommendationOllamaOptions
        {
            Endpoint = GetEnvironmentVariableOrDefault(
                "ALCOPILOT_RECOMMENDATION_AGENT_EVAL_OLLAMA_ENDPOINT",
                devSettings.Endpoint ?? "http://localhost:11434"),
            ModelId = GetEnvironmentVariableOrDefault(
                "ALCOPILOT_RECOMMENDATION_AGENT_EVAL_OLLAMA_MODEL",
                devSettings.ModelId ?? "gemma4:e4b"),
        };
    }

    private static EvalOllamaSettings ReadDevOllamaSettings()
    {
        var devSettingsPath = FindFileAboveCurrentDirectory(
            Path.Combine("server", "src", "AlCopilot.Host", "appsettings.Development.json"));
        if (devSettingsPath is null)
        {
            return new EvalOllamaSettings(null, null);
        }

        using var stream = File.OpenRead(devSettingsPath);
        using var document = JsonDocument.Parse(stream);
        if (!document.RootElement.TryGetProperty("Recommendation", out var recommendation)
            || !recommendation.TryGetProperty("Ollama", out var ollama))
        {
            return new EvalOllamaSettings(null, null);
        }

        var endpoint = TryGetString(ollama, "Endpoint");
        var modelId = TryGetString(ollama, "ModelId");

        return new EvalOllamaSettings(endpoint, modelId);
    }

    private static string? FindFileAboveCurrentDirectory(string relativePath)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            ? property.GetString()
            : null;
    }

    private static string GetEnvironmentVariableOrDefault(string name, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static float? TryGetFloatEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return float.TryParse(value, out var parsed) ? parsed : null;
    }

    private sealed record EvalOllamaSettings(
        string? Endpoint,
        string? ModelId);

    private sealed record RecommendationAgentEvalRuntime(
        RecommendationConversationService Service,
        RecommendationDbContext DbContext,
        IRecommendationNarratorAgentFactory AgentFactory);

    private sealed class EvalHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "AlCopilot.Recommendation.EvalTests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    private sealed class EvalSemanticSearchService : IRecommendationSemanticSearchService
    {
        public Task<RecommendationSemanticSearchResult> SearchAsync(
            string customerMessage,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RecommendationSemanticSearchResult.Empty);
        }
    }

    private sealed class EvalCatalogFuzzyLookupService(RecommendationAgentEvalSeedCatalog catalog)
        : IRecommendationCatalogFuzzyLookupService
    {
        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindDrinkMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<RecommendationFuzzyMatch>>(
                catalog.Drinks
                    .Select(drink => new RecommendationFuzzyMatch(
                        drink.Id,
                        drink.Name,
                        CalculateSimilarity(searchText, drink.Name)))
                    .Where(match => match.Similarity >= 0.55d)
                    .OrderByDescending(match => match.Similarity)
                    .ThenBy(match => match.Name, StringComparer.OrdinalIgnoreCase)
                    .Take(5)
                    .ToList());
        }

        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindIngredientMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<RecommendationFuzzyMatch>>(
                catalog.Ingredients
                    .Select(ingredient => new RecommendationFuzzyMatch(
                        ingredient.Value,
                        ingredient.Key,
                        CalculateSimilarity(searchText, ingredient.Key)))
                    .Where(match => match.Similarity >= 0.55d)
                    .OrderByDescending(match => match.Similarity)
                    .ThenBy(match => match.Name, StringComparer.OrdinalIgnoreCase)
                    .Take(5)
                    .ToList());
        }

        private static double CalculateSimilarity(string left, string right)
        {
            var normalizedLeft = Normalize(left);
            var normalizedRight = Normalize(right);
            if (normalizedLeft.Length == 0 || normalizedRight.Length == 0)
            {
                return 0d;
            }

            if (string.Equals(normalizedLeft, normalizedRight, StringComparison.Ordinal))
            {
                return 1d;
            }

            if (normalizedLeft.Contains(normalizedRight, StringComparison.Ordinal)
                || normalizedRight.Contains(normalizedLeft, StringComparison.Ordinal))
            {
                return 0.9d;
            }

            var distance = CalculateLevenshteinDistance(normalizedLeft, normalizedRight);
            return 1d - ((double)distance / Math.Max(normalizedLeft.Length, normalizedRight.Length));
        }

        private static string Normalize(string value)
        {
            return new string(value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
        }

        private static int CalculateLevenshteinDistance(string left, string right)
        {
            var distances = new int[left.Length + 1, right.Length + 1];
            for (var i = 0; i <= left.Length; i++)
            {
                distances[i, 0] = i;
            }

            for (var j = 0; j <= right.Length; j++)
            {
                distances[0, j] = j;
            }

            for (var i = 1; i <= left.Length; i++)
            {
                for (var j = 1; j <= right.Length; j++)
                {
                    var cost = left[i - 1] == right[j - 1] ? 0 : 1;
                    distances[i, j] = Math.Min(
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost);
                }
            }

            return distances[left.Length, right.Length];
        }
    }
}

internal sealed record RecommendationAgentEvalResult(
    string CaseName,
    int TurnNumber,
    Guid SessionId,
    string Response,
    IReadOnlyCollection<string> ToolNames)
{
    public int RepetitionNumber => TurnNumber;
}
