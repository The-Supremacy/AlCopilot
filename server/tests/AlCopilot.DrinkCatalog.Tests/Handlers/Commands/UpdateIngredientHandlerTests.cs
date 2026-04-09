using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class UpdateIngredientHandlerTests
{
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateIngredientHandler _handler;

    public UpdateIngredientHandlerTests()
    {
        _handler = new UpdateIngredientHandler(_ingredientRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenFound_UpdatesBrandsAndReturnsTrue()
    {
        var ingredient = Ingredient.Create(IngredientName.Create("Vodka"), Guid.NewGuid(), ["Absolut"]);
        _ingredientRepository.GetByIdAsync(ingredient.Id, Arg.Any<CancellationToken>()).Returns(ingredient);

        var result = await _handler.Handle(
            new UpdateIngredientCommand(ingredient.Id, ["Grey Goose"]), CancellationToken.None);

        result.ShouldBeTrue();
        ingredient.NotableBrands.ShouldBe(["Grey Goose"]);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFalse()
    {
        _ingredientRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Ingredient?)null);

        var result = await _handler.Handle(
            new UpdateIngredientCommand(Guid.NewGuid(), []), CancellationToken.None);

        result.ShouldBeFalse();
    }
}
