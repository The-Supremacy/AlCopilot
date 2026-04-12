using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Queries;

public sealed class GetIngredientsHandlerTests
{
    private readonly IIngredientRepository _repository = Substitute.For<IIngredientRepository>();
    private readonly GetIngredientsHandler _handler;

    public GetIngredientsHandlerTests()
    {
        _handler = new GetIngredientsHandler(_repository);
    }

    [Fact]
    public async Task Handle_ReturnsIngredients()
    {
        var expected = new List<IngredientDto>();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetIngredientsQuery(), CancellationToken.None);

        result.ShouldBe(expected);
    }
}
