namespace AlCopilot.CustomerProfile.Contracts.DTOs;

public sealed record CustomerProfileDto(
    List<Guid> FavoriteIngredientIds,
    List<Guid> DislikedIngredientIds,
    List<Guid> ProhibitedIngredientIds,
    List<Guid> OwnedIngredientIds);
