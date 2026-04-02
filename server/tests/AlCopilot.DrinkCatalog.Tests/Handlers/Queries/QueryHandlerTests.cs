using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Models;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Queries;

public sealed class GetDrinksHandlerTests
{
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly GetDrinksHandler _handler;

    public GetDrinksHandlerTests()
    {
        _handler = new GetDrinksHandler(_drinkRepository);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResult()
    {
        var expected = new PagedResult<DrinkDto>([], 0, 1, 20);
        _drinkRepository.GetPagedAsync(Arg.Any<DrinkFilter>(), Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetDrinksQuery(new DrinkFilter()), CancellationToken.None);

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_WithTagFilter_PassesTagIds()
    {
        var tagIds = new List<Guid> { Guid.NewGuid() };
        var expected = new PagedResult<DrinkDto>([], 0, 1, 20);
        _drinkRepository.GetPagedAsync(
            Arg.Is<DrinkFilter>(f => f.SearchQuery == null && f.TagIds != null && f.TagIds.SequenceEqual(tagIds) && f.Page == 1 && f.PageSize == 20),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetDrinksQuery(new DrinkFilter(null, tagIds, 1, 20)), CancellationToken.None);

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_WithSearchQuery_PassesSearchText()
    {
        var expected = new PagedResult<DrinkDto>([], 0, 1, 20);
        _drinkRepository.GetPagedAsync(
            Arg.Is<DrinkFilter>(f => f.SearchQuery == "mojito" && f.TagIds == null && f.Page == 1 && f.PageSize == 20),
            Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetDrinksQuery(new DrinkFilter("mojito", null, 1, 20)), CancellationToken.None);

        result.ShouldBe(expected);
    }
}

public sealed class GetDrinkByIdHandlerTests
{
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly GetDrinkByIdHandler _handler;

    public GetDrinkByIdHandlerTests()
    {
        _handler = new GetDrinkByIdHandler(_drinkRepository);
    }

    [Fact]
    public async Task Handle_WhenFound_ReturnsDetail()
    {
        var id = Guid.NewGuid();
        var expected = new DrinkDetailDto(id, "Test", null, null, [], []);
        _drinkRepository.GetDetailByIdAsync(id, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetDrinkByIdQuery(id), CancellationToken.None);

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNull()
    {
        _drinkRepository.GetDetailByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DrinkDetailDto?)null);

        var result = await _handler.Handle(new GetDrinkByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}

public sealed class GetTagsHandlerTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly GetTagsHandler _handler;

    public GetTagsHandlerTests()
    {
        _handler = new GetTagsHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_ReturnsAllTags()
    {
        var expected = new List<TagDto> { new(Guid.NewGuid(), "Classic", 5) };
        _tagRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetTagsQuery(), CancellationToken.None);

        result.ShouldBe(expected);
    }
}

public sealed class GetIngredientCategoriesHandlerTests
{
    private readonly IIngredientCategoryRepository _repo = Substitute.For<IIngredientCategoryRepository>();
    private readonly GetIngredientCategoriesHandler _handler;

    public GetIngredientCategoriesHandlerTests()
    {
        _handler = new GetIngredientCategoriesHandler(_repo);
    }

    [Fact]
    public async Task Handle_ReturnsAllCategories()
    {
        var expected = new List<IngredientCategoryDto> { new(Guid.NewGuid(), "Spirits") };
        _repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetIngredientCategoriesQuery(), CancellationToken.None);

        result.ShouldBe(expected);
    }
}

public sealed class GetIngredientsHandlerTests
{
    private readonly IIngredientRepository _repo = Substitute.For<IIngredientRepository>();
    private readonly GetIngredientsHandler _handler;

    public GetIngredientsHandlerTests()
    {
        _handler = new GetIngredientsHandler(_repo);
    }

    [Fact]
    public async Task Handle_ReturnsIngredients()
    {
        var expected = new List<IngredientDto>();
        _repo.GetAllAsync(null, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetIngredientsQuery(), CancellationToken.None);

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_PassesCategoryId()
    {
        var categoryId = Guid.NewGuid();
        var expected = new List<IngredientDto>();
        _repo.GetAllAsync(categoryId, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetIngredientsQuery(categoryId), CancellationToken.None);

        result.ShouldBe(expected);
    }
}
