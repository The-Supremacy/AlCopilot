using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Commands;

// --- Drinks ---

public sealed record RecipeEntryInput(Guid IngredientId, string Quantity, string? RecommendedBrand);

public sealed record CreateDrinkCommand(
    string Name,
    string? Description,
    string? ImageUrl,
    List<Guid> TagIds,
    List<RecipeEntryInput> RecipeEntries) : IRequest<Guid>;

public sealed record UpdateDrinkCommand(
    Guid DrinkId,
    string Name,
    string? Description,
    string? ImageUrl,
    List<Guid> TagIds,
    List<RecipeEntryInput> RecipeEntries) : IRequest<bool>;

public sealed record DeleteDrinkCommand(Guid DrinkId) : IRequest<bool>;

// --- Tags ---

public sealed record CreateTagCommand(string Name) : IRequest<Guid>;

public sealed record DeleteTagCommand(Guid TagId) : IRequest<bool>;

// --- Ingredient Categories ---

public sealed record CreateIngredientCategoryCommand(string Name) : IRequest<Guid>;

// --- Ingredients ---

public sealed record CreateIngredientCommand(
    string Name,
    Guid CategoryId,
    List<string> NotableBrands) : IRequest<Guid>;

public sealed record UpdateIngredientCommand(
    Guid IngredientId,
    List<string> NotableBrands) : IRequest<bool>;
