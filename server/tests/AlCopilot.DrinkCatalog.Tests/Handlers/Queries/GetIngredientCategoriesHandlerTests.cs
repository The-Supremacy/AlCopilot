using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Queries;

public sealed class GetIngredientCategoriesHandlerTests
{
    private readonly IIngredientCategoryRepository _repository = Substitute.For<IIngredientCategoryRepository>();
    private readonly GetIngredientCategoriesHandler _handler;

    public GetIngredientCategoriesHandlerTests()
    {
        _handler = new GetIngredientCategoriesHandler(_repository);
    }

    [Fact]
    public async Task Handle_ReturnsAllCategories()
    {
        var expected = new List<IngredientCategoryDto> { new(Guid.NewGuid(), "Spirits") };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetIngredientCategoriesQuery(), CancellationToken.None);

        result.ShouldBe(expected);
    }
}
