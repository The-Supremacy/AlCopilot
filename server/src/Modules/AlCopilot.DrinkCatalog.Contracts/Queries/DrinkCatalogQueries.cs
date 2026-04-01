using AlCopilot.DrinkCatalog.Contracts.DTOs;
using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Queries;

public sealed record GetDrinksQuery(
    List<Guid>? TagIds,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<DrinkDto>>;

public sealed record GetDrinkByIdQuery(Guid DrinkId) : IRequest<DrinkDetailDto?>;

public sealed record SearchDrinksQuery(
    string Query,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<DrinkDto>>;

public sealed record GetTagsQuery : IRequest<List<TagDto>>;

public sealed record GetIngredientCategoriesQuery : IRequest<List<IngredientCategoryDto>>;

public sealed record GetIngredientsQuery(Guid? CategoryId = null) : IRequest<List<IngredientDto>>;
