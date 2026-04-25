namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationAgentTurnState
{
    public Guid RecommendationSessionId { get; set; }

    public string? CustomerMessage { get; set; }

    public RecommendationRequestQueryAnalysis? RequestAnalysis { get; set; }

    public RecommendationRunInputs? Inputs { get; set; }

    public RecommendationSemanticSearchResult SemanticSearchResult { get; set; } =
        RecommendationSemanticSearchResult.Empty;

    public RecommendationRequestIntent? Intent { get; set; }

    public RecommendationRunContext? RunContext { get; set; }
}
