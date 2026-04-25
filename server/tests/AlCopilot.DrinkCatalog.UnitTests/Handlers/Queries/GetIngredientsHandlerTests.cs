using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.UnitTests.Handlers.Queries;

public sealed class GetIngredientsHandlerTests
{
    private readonly IIngredientQueryService _ingredientQueryService = Substitute.For<IIngredientQueryService>();
    private readonly GetIngredientsHandler _handler;

    public GetIngredientsHandlerTests()
    {
        _handler = new GetIngredientsHandler(_ingredientQueryService);
    }

    [Fact]
    public async Task Handle_ReturnsIngredients()
    {
        var expected = new List<IngredientDto>();
        _ingredientQueryService.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetIngredientsQuery(), CancellationToken.None);

        result.ShouldBe(expected);
    }
}
