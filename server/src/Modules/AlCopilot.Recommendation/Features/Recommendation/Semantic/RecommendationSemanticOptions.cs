namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class RecommendationSemanticOptions
{
    public const string SectionName = "Recommendation:Semantic";

    public bool Enabled { get; init; } = true;

    public string QdrantEndpoint { get; init; } = "http://localhost:6334";

    public string? QdrantApiKey { get; init; }

    public string CollectionName { get; init; } = "recommendation-semantic-catalog";

    public int SearchLimit { get; init; } = 18;

    public double DescriptionMinScore { get; init; } = 0.55d;

    public double DescriptionWeight { get; init; } = 1.5d;

    public string EmbeddingModelId { get; init; } = "nomic-embed-text";
}
