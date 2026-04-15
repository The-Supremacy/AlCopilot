using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Commands;

public sealed record CreateIngredientCommand(
    string Name,
    List<string> NotableBrands) : IRequest<Guid>;

public sealed record UpdateIngredientCommand(
    Guid IngredientId,
    string Name,
    List<string> NotableBrands) : IRequest<bool>;

public sealed record DeleteIngredientCommand(Guid IngredientId) : IRequest<bool>;
