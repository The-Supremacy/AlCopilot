namespace AlCopilot.DrinkCatalog.Contracts.DTOs;

public sealed record TagDto(Guid Id, string Name, int DrinkCount);

public sealed record IngredientDto(Guid Id, string Name, List<string> NotableBrands);

public sealed record RecipeEntryDto(IngredientDto Ingredient, string Quantity, string? RecommendedBrand);

public sealed record DrinkDto(
    Guid Id,
    string Name,
    string? Category,
    string? Description,
    string? Method,
    string? Garnish,
    string? ImageUrl,
    List<TagDto> Tags);

public sealed record DrinkDetailDto(
    Guid Id,
    string Name,
    string? Category,
    string? Description,
    string? Method,
    string? Garnish,
    string? ImageUrl,
    List<TagDto> Tags,
    List<RecipeEntryDto> RecipeEntries);

public sealed record FuzzyDrinkMatchDto(
    Guid DrinkId,
    string DrinkName,
    double Similarity);

public sealed record FuzzyIngredientMatchDto(
    Guid IngredientId,
    string IngredientName,
    double Similarity);
