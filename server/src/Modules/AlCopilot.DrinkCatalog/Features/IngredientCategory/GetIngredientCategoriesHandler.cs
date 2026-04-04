using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.IngredientCategory;

public sealed class GetIngredientCategoriesHandler(IIngredientCategoryRepository categoryRepository)
    : IRequestHandler<GetIngredientCategoriesQuery, List<IngredientCategoryDto>>
{
    public async ValueTask<List<IngredientCategoryDto>> Handle(
        GetIngredientCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await categoryRepository.GetAllAsync(cancellationToken);
    }
}
