namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

public sealed class RecommendationOllamaOptions
{
    public const string SectionName = "Recommendation:Ollama";

    public string Endpoint { get; init; } = "http://localhost:11434";

    public string ModelId { get; init; } = "gemma4:e4b";
}
