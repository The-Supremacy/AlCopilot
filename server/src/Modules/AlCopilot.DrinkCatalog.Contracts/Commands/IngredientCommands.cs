using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Commands;

public sealed record CreateIngredientCommand(
    string Name,
    List<string> NotableBrands,
    string? IngredientGroup = null) : IRequest<Guid>;

public sealed record UpdateIngredientCommand(
    Guid IngredientId,
    string Name,
    List<string> NotableBrands,
    string? IngredientGroup = null) : IRequest<bool>;

public sealed record DeleteIngredientCommand(Guid IngredientId) : IRequest<bool>;
