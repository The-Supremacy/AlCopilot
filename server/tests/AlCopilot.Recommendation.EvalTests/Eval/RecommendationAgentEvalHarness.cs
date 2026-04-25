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
            $"maf-eval-{evalCase.Name}-{repetitionNumber}",
            null,
            evalCase.Prompt,
            evalCase.Name,
            repetitionNumber,
            cancellationToken);
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
        string customerId,
        Guid? sessionId,
        string prompt,
        string caseName,
        int turnNumber,
        CancellationToken cancellationToken)
    {
        var session = await service.SendMessageAsync(
            customerId,
            sessionId,
            prompt,
            cancellationToken);
        var assistantTurn = session.Turns.Last(turn => string.Equals(turn.Role, "assistant", StringComparison.Ordinal));

        return new RecommendationAgentEvalResult(
            caseName,
            turnNumber,
            session.SessionId,
            assistantTurn.Content,
            assistantTurn.ToolInvocations);
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
        var toolInvocationRecorder = new RecommendationToolInvocationRecorder();
        var runInputsQueryService = new RecommendationRunInputsQueryService(mediator);
        var requestIntentResolver = new RecommendationRequestIntentResolver(
            fuzzyLookupService,
            Options.Create(new RecommendationSemanticOptions
            {
                Enabled = false,
            }));
        var strategyFactory = new RecommendationChatClientStrategyFactory(
            Options.Create(BuildLlmOptions()),
            Options.Create(BuildOllamaOptions()));
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var drinkSearchTool = new RecommendationDrinkSearchTool(
            mediator,
            fuzzyLookupService,
            toolInvocationRecorder,
            executionTraceRecorder);
        var ingredientLookupTool = new RecommendationIngredientLookupTool(
            mediator,
            fuzzyLookupService,
            toolInvocationRecorder,
            executionTraceRecorder);
        var recipeLookupTool = new RecommendationRecipeLookupTool(
            mediator,
            toolInvocationRecorder,
            executionTraceRecorder);
        var agentFactory = new RecommendationNarratorAgentFactory(
            strategyFactory,
            loggerFactory,
            Options.Create(new RecommendationObservabilityOptions()),
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
        var repository = new EvalChatSessionRepository();
        var service = new RecommendationConversationService(
            repository,
            agentFactory,
            new RecommendationAgentSessionStore(),
            executionTraceRecorder,
            toolInvocationRecorder,
            new EvalRecommendationUnitOfWork(),
            new EvalHostEnvironment(),
            Options.Create(new RecommendationObservabilityOptions()),
            loggerFactory.CreateLogger<RecommendationConversationService>());

        return new RecommendationAgentEvalRuntime(service);
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
            MaxHistoryMessages = devSettings.MaxHistoryMessages ?? 24,
        };
    }

    private static EvalOllamaSettings ReadDevOllamaSettings()
    {
        var devSettingsPath = FindFileAboveCurrentDirectory(
            Path.Combine("server", "src", "AlCopilot.Host", "appsettings.Development.json"));
        if (devSettingsPath is null)
        {
            return new EvalOllamaSettings(null, null, null);
        }

        using var stream = File.OpenRead(devSettingsPath);
        using var document = JsonDocument.Parse(stream);
        if (!document.RootElement.TryGetProperty("Recommendation", out var recommendation)
            || !recommendation.TryGetProperty("Ollama", out var ollama))
        {
            return new EvalOllamaSettings(null, null, null);
        }

        var endpoint = TryGetString(ollama, "Endpoint");
        var modelId = TryGetString(ollama, "ModelId");
        var maxHistoryMessages = TryGetInt32(ollama, "MaxHistoryMessages");

        return new EvalOllamaSettings(endpoint, modelId, maxHistoryMessages);
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

    private static int? TryGetInt32(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property)
            && property.TryGetInt32(out var value)
            ? value
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
        string? ModelId,
        int? MaxHistoryMessages);

    private sealed record RecommendationAgentEvalRuntime(RecommendationConversationService Service);

    private sealed class EvalChatSessionRepository : IChatSessionRepository
    {
        private readonly List<ChatSession> sessions = [];

        public Task<ChatSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var session = sessions.FirstOrDefault(candidate => candidate.Id == id);
            return Task.FromResult(session);
        }

        public Task<ChatSession?> GetByCustomerSessionIdAsync(
            string customerId,
            Guid sessionId,
            CancellationToken cancellationToken = default)
        {
            var session = sessions.FirstOrDefault(candidate =>
                candidate.Id == sessionId
                && string.Equals(candidate.CustomerId, customerId, StringComparison.Ordinal));
            return Task.FromResult(session);
        }

        public void Add(ChatSession aggregate)
        {
            sessions.Add(aggregate);
        }

        public void Remove(ChatSession aggregate)
        {
            sessions.Remove(aggregate);
        }
    }

    private sealed class EvalRecommendationUnitOfWork : IRecommendationUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }

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
    IReadOnlyCollection<RecommendationToolInvocationDto> ToolInvocations)
{
    public int RepetitionNumber => TurnNumber;
}
