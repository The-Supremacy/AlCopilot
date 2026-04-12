using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Commands;

public sealed record RecipeEntryInput(Guid IngredientId, string Quantity, string? RecommendedBrand);

public sealed record CreateDrinkCommand(
    string Name,
    string? Category,
    string? Description,
    string? Method,
    string? Garnish,
    string? ImageUrl,
    List<Guid> TagIds,
    List<RecipeEntryInput> RecipeEntries) : IRequest<Guid>;

public sealed record UpdateDrinkCommand(
    Guid DrinkId,
    string Name,
    string? Category,
    string? Description,
    string? Method,
    string? Garnish,
    string? ImageUrl,
    List<Guid> TagIds,
    List<RecipeEntryInput> RecipeEntries) : IRequest<bool>;

public sealed record DeleteDrinkCommand(Guid DrinkId) : IRequest<bool>;
