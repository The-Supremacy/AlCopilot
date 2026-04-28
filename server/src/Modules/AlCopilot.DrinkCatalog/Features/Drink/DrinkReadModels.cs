namespace AlCopilot.DrinkCatalog.Features.Drink;

internal sealed record DrinkListReadModel(
    Guid Id,
    string Name,
    string? Category,
    string? Description,
    string? Method,
    string? Garnish,
    string? ImageUrl,
    List<TagReadModel> Tags);

internal sealed record DrinkDetailReadModel(
    Guid Id,
    string Name,
    string? Category,
    string? Description,
    string? Method,
    string? Garnish,
    string? ImageUrl,
    List<TagReadModel> Tags,
    List<RecipeEntryReadModel> RecipeEntries);

internal sealed record TagReadModel(Guid Id, string Name);

internal sealed record IngredientReadModel(
    Guid Id,
    string Name,
    string? IngredientGroup,
    List<string> NotableBrands);

internal sealed record RecipeEntryReadModel(Guid IngredientId, string Quantity, string? RecommendedBrand);
