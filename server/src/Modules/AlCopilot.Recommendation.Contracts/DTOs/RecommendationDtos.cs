namespace AlCopilot.Recommendation.Contracts.DTOs;

using AlCopilot.DrinkCatalog.Contracts.DTOs;

public sealed record RecommendationItemDto(
    Guid DrinkId,
    string DrinkName,
    string? Description,
    List<string> MissingIngredientNames,
    List<string> MatchedSignals,
    int Score,
    List<RecommendationRecipeEntryDto>? RecipeEntries = null);

public sealed record RecommendationRecipeEntryDto(
    string IngredientName,
    string Quantity,
    bool IsOwned);

public sealed record RecommendationGroupDto(
    string Key,
    string Label,
    List<RecommendationItemDto> Items);

public sealed record RecommendationTurnFeedbackDto(
    string Rating,
    string? Comment,
    DateTimeOffset CreatedAtUtc);

public sealed record RecommendationTurnDto(
    Guid TurnId,
    int Sequence,
    string Role,
    string Content,
    List<RecommendationGroupDto> RecommendationGroups,
    RecommendationTurnFeedbackDto? Feedback,
    DateTimeOffset CreatedAtUtc);

public sealed record RecommendationSessionDto(
    Guid SessionId,
    string Title,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    List<RecommendationTurnDto> Turns);

public sealed record RecommendationSessionSummaryDto(
    Guid SessionId,
    string Title,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string LastAssistantMessage);

public sealed record SubmitRecommendationMessageResultDto(Guid SessionId);

public sealed record RecommendationSemanticCatalogIndexResultDto(
    int DrinkCount,
    int PointCount);
