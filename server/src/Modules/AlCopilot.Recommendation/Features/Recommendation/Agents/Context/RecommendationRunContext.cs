using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

public sealed record RecommendationRunContext(
    RecommendationRequestIntent Intent,
    CustomerProfileDto Profile,
    IReadOnlyCollection<RecommendationGroupDto> RecommendationGroups,
    IReadOnlyDictionary<Guid, string> IngredientNames,
    IReadOnlyCollection<RecommendationRunContextGroup> Groups);

public sealed record RecommendationRunContextGroup(
    string Key,
    string Label,
    IReadOnlyCollection<RecommendationRunContextItem> Items);

public sealed record RecommendationRunContextItem(
    Guid DrinkId,
    string DrinkName,
    string? Description,
    IReadOnlyCollection<string> OwnedIngredientNames,
    IReadOnlyCollection<string> MissingIngredientNames,
    IReadOnlyCollection<string> RecipeIngredientNames,
    string? Method,
    string? Garnish,
    IReadOnlyCollection<string> MatchedSignals,
    int Score);
