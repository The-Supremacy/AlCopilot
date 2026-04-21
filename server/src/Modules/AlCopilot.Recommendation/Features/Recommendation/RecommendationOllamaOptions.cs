namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class RecommendationOllamaOptions
{
    public const string SectionName = "Recommendation:Ollama";

    public string Endpoint { get; init; } = "http://localhost:11434";

    public string ModelId { get; init; } = "gemma4:e4b";

    public int MaxHistoryMessages { get; init; } = 24;

    public int MaxHistoryTurns { get; init; }

    internal int GetEffectiveMaxHistoryMessages()
    {
        if (MaxHistoryMessages > 0)
        {
            return MaxHistoryMessages;
        }

        return Math.Max(1, MaxHistoryTurns) * 2;
    }
}
