namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

public sealed class RecommendationLlmOptions
{
    public const string SectionName = "Recommendation:Llm";
    public const string OllamaProvider = "ollama";

    public string Provider { get; init; } = OllamaProvider;
}
