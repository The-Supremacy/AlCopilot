namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class RecommendationOllamaOptions
{
    public const string SectionName = "Recommendation:Ollama";

    public string Endpoint { get; init; } = "http://localhost:11434";

    public string ModelId { get; init; } = "llama3.2:latest";

    public int MaxHistoryTurns { get; init; } = 12;

    public bool EnableReadOnlyTools { get; init; }
}
