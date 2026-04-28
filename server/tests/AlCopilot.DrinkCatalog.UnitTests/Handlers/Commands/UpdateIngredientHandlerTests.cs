using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.UnitTests.Handlers.Commands;

public sealed class UpdateIngredientHandlerTests
{
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly UpdateIngredientHandler _handler;

    public UpdateIngredientHandlerTests()
    {
        _handler = new UpdateIngredientHandler(_ingredientRepository, new AuditLogWriter(_auditRepository), _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenFound_UpdatesNameAndBrandsAndReturnsTrue()
    {
        var ingredient = Ingredient.Create(IngredientName.Create("Vodka"), ["Absolut"]);
        _ingredientRepository.GetByIdAsync(ingredient.Id, Arg.Any<CancellationToken>()).Returns(ingredient);

        var result = await _handler.Handle(
            new UpdateIngredientCommand(ingredient.Id, "Premium Vodka", ["Grey Goose"], "Vodka"), CancellationToken.None);

        result.ShouldBeTrue();
        ingredient.Name.Value.ShouldBe("Premium Vodka");
        ingredient.NotableBrands.ShouldBe(["Grey Goose"]);
        ingredient.GetGroupName().ShouldBe("Vodka");
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFalse()
    {
        _ingredientRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Ingredient?)null);

        var result = await _handler.Handle(
            new UpdateIngredientCommand(Guid.NewGuid(), "Vodka", []), CancellationToken.None);

        result.ShouldBeFalse();
    }
}
