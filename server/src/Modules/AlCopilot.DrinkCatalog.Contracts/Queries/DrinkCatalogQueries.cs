using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Queries;

public sealed record DrinkFilter(
    string? SearchQuery = null,
    IReadOnlyList<Guid>? TagIds = null,
    int Page = 1,
    int PageSize = 20);

public sealed record GetDrinksQuery(
    DrinkFilter Filter) : IRequest<PagedResult<DrinkDto>>;

public sealed record GetDrinkByIdQuery(Guid DrinkId) : IRequest<DrinkDetailDto?>;

public sealed record GetTagsQuery : IRequest<List<TagDto>>;

public sealed record GetIngredientCategoriesQuery : IRequest<List<IngredientCategoryDto>>;

public sealed record GetIngredientsQuery(Guid? CategoryId = null) : IRequest<List<IngredientDto>>;
