namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class RecommendationSemanticOptions
{
    public const string SectionName = "Recommendation:Semantic";

    public bool Enabled { get; init; } = true;

    public string QdrantEndpoint { get; init; } = "http://localhost:6334";

    public string? QdrantApiKey { get; init; }

    public string CollectionName { get; init; } = "recommendation-semantic-catalog";

    public int SearchLimit { get; init; } = 18;

    public double NameMatchMinScore { get; init; } = 0.72d;

    public double IngredientMatchMinScore { get; init; } = 0.72d;

    public double FacetMatchMinScoreGap { get; init; } = 0.05d;

    public double NameWeight { get; init; } = 1.25d;

    public double IngredientWeight { get; init; } = 1.0d;

    public double DescriptionWeight { get; init; } = 1.5d;

    public string EmbeddingModelId { get; init; } = "nomic-embed-text";
}
