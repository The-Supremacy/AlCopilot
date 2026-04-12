using AlCopilot.DrinkCatalog.Contracts.DTOs;
using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Queries;

public sealed record GetIngredientsQuery() : IRequest<List<IngredientDto>>;
