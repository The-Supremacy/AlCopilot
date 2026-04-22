using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

public sealed class RecommendationLlmOptions
{
    public const string SectionName = "Recommendation:Llm";
    public const string OllamaProvider = "ollama";

    public string Provider { get; init; } = OllamaProvider;

    public RecommendationSamplingOptions Sampling { get; init; } = new();

    public RecommendationReasoningOptions Reasoning { get; init; } = new();
}

public sealed class RecommendationSamplingOptions
{
    public float Temperature { get; init; } = 0.2f;

    public float TopP { get; init; } = 0.9f;

    public int? TopK { get; init; }
}

public sealed class RecommendationReasoningOptions
{
    public bool Enabled { get; init; }

    public ReasoningEffort? Effort { get; init; } = ReasoningEffort.Low;

    public ReasoningOutput Output { get; init; } = ReasoningOutput.None;
}
