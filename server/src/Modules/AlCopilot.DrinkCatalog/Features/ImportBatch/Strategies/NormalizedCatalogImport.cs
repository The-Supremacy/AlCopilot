namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;

public sealed record NormalizedCatalogImport(
    List<NormalizedTagImport> Tags,
    List<NormalizedIngredientImport> Ingredients,
    List<NormalizedDrinkImport> Drinks);

public sealed record NormalizedTagImport(string Name);

public sealed record NormalizedIngredientImport(
    string Name,
    List<string> NotableBrands,
    string? IngredientGroup = null);

public sealed record NormalizedDrinkImport(
    string Name,
    string? Category,
    string? Description,
    string? Method,
    string? Garnish,
    string? ImageUrl,
    List<string> TagNames,
    List<NormalizedDrinkRecipeEntryImport> RecipeEntries);

public sealed record NormalizedDrinkRecipeEntryImport(
    string IngredientName,
    string Quantity,
    string? RecommendedBrand);
