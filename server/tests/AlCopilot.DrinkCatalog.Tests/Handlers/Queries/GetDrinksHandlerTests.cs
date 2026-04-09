using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Drink;
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
