using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class DeleteDrinkHandlerTests
{
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteDrinkHandler _handler;

    public DeleteDrinkHandlerTests()
    {
        _handler = new DeleteDrinkHandler(_drinkRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenFound_SoftDeletesAndReturnsTrue()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));
        _drinkRepository.GetByIdAsync(drink.Id, Arg.Any<CancellationToken>()).Returns(drink);

        var result = await _handler.Handle(new DeleteDrinkCommand(drink.Id), CancellationToken.None);

        result.ShouldBeTrue();
        drink.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFalse()
    {
        _drinkRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Drink?)null);

        var result = await _handler.Handle(new DeleteDrinkCommand(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeFalse();
    }
}
