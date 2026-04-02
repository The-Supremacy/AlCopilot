using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Queries;

public sealed class GetIngredientsHandler(IIngredientRepository ingredientRepository)
    : IRequestHandler<GetIngredientsQuery, List<IngredientDto>>
{
    public async ValueTask<List<IngredientDto>> Handle(
        GetIngredientsQuery request, CancellationToken cancellationToken)
    {
        return await ingredientRepository.GetAllAsync(request.CategoryId, cancellationToken);
    }
}
