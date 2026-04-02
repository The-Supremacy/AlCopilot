using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Queries;

public sealed class GetIngredientCategoriesHandler(IIngredientCategoryRepository categoryRepository)
    : IRequestHandler<GetIngredientCategoriesQuery, List<IngredientCategoryDto>>
{
    public async ValueTask<List<IngredientCategoryDto>> Handle(
        GetIngredientCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await categoryRepository.GetAllAsync(cancellationToken);
    }
}
