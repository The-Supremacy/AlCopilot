using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class UpdateDrinkHandlerTests
{
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateDrinkHandler _handler;

    public UpdateDrinkHandlerTests()
    {
        _handler = new UpdateDrinkHandler(_drinkRepository, _tagRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFalse()
    {
        _drinkRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Drink?)null);

        var result = await _handler.Handle(
            new UpdateDrinkCommand(Guid.NewGuid(), "Test", null, null, [], []),
            CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsTrue()
    {
        var drink = Drink.Create(DrinkName.Create("Old"), null, ImageUrl.Create(null));
        _drinkRepository.GetByIdAsync(drink.Id, Arg.Any<CancellationToken>()).Returns(drink);
        _drinkRepository.ExistsByNameAsync("New", drink.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(
            new UpdateDrinkCommand(drink.Id, "New", "Desc", null, [], []),
            CancellationToken.None);

        result.ShouldBeTrue();
        drink.Name.Value.ShouldBe("New");
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        var drink = Drink.Create(DrinkName.Create("Old"), null, ImageUrl.Create(null));
        _drinkRepository.GetByIdAsync(drink.Id, Arg.Any<CancellationToken>()).Returns(drink);
        _drinkRepository.ExistsByNameAsync("Taken", drink.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(
                new UpdateDrinkCommand(drink.Id, "Taken", null, null, [], []),
                CancellationToken.None).AsTask());
    }
}
