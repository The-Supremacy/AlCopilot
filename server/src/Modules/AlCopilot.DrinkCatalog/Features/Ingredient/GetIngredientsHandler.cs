using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public sealed class GetIngredientsHandler(IIngredientQueryService ingredientQueryService)
    : IRequestHandler<GetIngredientsQuery, List<IngredientDto>>
{
    public async ValueTask<List<IngredientDto>> Handle(
        GetIngredientsQuery request, CancellationToken cancellationToken)
    {
        return await ingredientQueryService.GetAllAsync(cancellationToken);
    }
}
