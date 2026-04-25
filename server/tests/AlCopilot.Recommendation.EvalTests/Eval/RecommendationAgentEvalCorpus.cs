using System.Text.Json;

namespace AlCopilot.Recommendation.EvalTests.Eval;

public sealed record RecommendationAgentEvalCorpus(
    int SchemaVersion,
    string CatalogFixture,
    IReadOnlyList<RecommendationAgentEvalCase> Cases,
    IReadOnlyList<RecommendationAgentEvalSessionCase> SessionCases)
{
    private const string CorpusPath = "TestData/maf-eval/recommendation-agent-eval.json";

    public static RecommendationAgentEvalCorpus Load()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            CorpusPath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Recommendation agent eval corpus was not found at '{path}'.", path);
        }

        using var stream = File.OpenRead(path);
        var corpus = JsonSerializer.Deserialize<RecommendationAgentEvalCorpus>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("Recommendation agent eval corpus is empty.");

        if (corpus.SchemaVersion != 1)
        {
            throw new InvalidOperationException(
                $"Recommendation agent eval corpus schema version '{corpus.SchemaVersion}' is not supported.");
        }

        if (!string.Equals(corpus.CatalogFixture, "recommendation-agent-eval-v1", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Recommendation agent eval catalog fixture '{corpus.CatalogFixture}' is not supported.");
        }

        return corpus;
    }
}

public sealed record RecommendationAgentEvalCase(
    string Name,
    string Prompt,
    RecommendationAgentEvalProfile Profile,
    IReadOnlyList<string> ExpectedResponseFragments,
    IReadOnlyList<string> ForbiddenResponseFragments,
    IReadOnlyList<string> ExpectedToolNames,
    IReadOnlyList<string> ForbiddenToolNames,
    int MaxToolCallCount,
    int RepetitionCount)
{
    public IReadOnlyList<string> ExpectedRecommendedDrinkNames { get; init; } = [];

    public IReadOnlyList<string> ForbiddenRecommendedDrinkNames { get; init; } = [];

    public override string ToString() => Name;
}

public sealed record RecommendationAgentEvalSessionCase(
    string Name,
    RecommendationAgentEvalProfile Profile,
    IReadOnlyList<RecommendationAgentEvalSessionTurn> Turns)
{
    public override string ToString() => Name;
}

public sealed record RecommendationAgentEvalSessionTurn(
    string Prompt,
    IReadOnlyList<string> ExpectedResponseFragments,
    IReadOnlyList<string> ForbiddenResponseFragments,
    IReadOnlyList<string> ExpectedToolNames,
    IReadOnlyList<string> ForbiddenToolNames,
    int MaxToolCallCount)
{
    public IReadOnlyList<string> ExpectedRecommendedDrinkNames { get; init; } = [];

    public IReadOnlyList<string> ForbiddenRecommendedDrinkNames { get; init; } = [];
}

public sealed record RecommendationAgentEvalProfile(
    IReadOnlyList<string> OwnedIngredientNames,
    IReadOnlyList<string> DislikedIngredientNames,
    IReadOnlyList<string> ProhibitedIngredientNames);
