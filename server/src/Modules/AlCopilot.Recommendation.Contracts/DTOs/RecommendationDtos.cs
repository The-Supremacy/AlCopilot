namespace AlCopilot.Recommendation.Contracts.DTOs;

public sealed record RecommendationItemDto(
    Guid DrinkId,
    string DrinkName,
    string? Description,
    List<string> MissingIngredientNames,
    List<string> MatchedSignals,
    int Score);

public sealed record RecommendationGroupDto(
    string Key,
    string Label,
    List<RecommendationItemDto> Items);

public sealed record RecommendationToolInvocationDto(
    string ToolName,
    string Purpose);

public sealed record RecommendationTurnDto(
    Guid TurnId,
    int Sequence,
    string Role,
    string Content,
    List<RecommendationGroupDto> RecommendationGroups,
    List<RecommendationToolInvocationDto> ToolInvocations,
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
