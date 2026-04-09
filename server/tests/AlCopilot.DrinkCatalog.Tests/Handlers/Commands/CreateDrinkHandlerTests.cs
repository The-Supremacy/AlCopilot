using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class CreateDrinkHandlerTests
{
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateDrinkHandler _handler;

    public CreateDrinkHandlerTests()
    {
        _handler = new CreateDrinkHandler(_drinkRepository, _tagRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesDrink()
    {
        _drinkRepository.ExistsByNameAsync("Margarita", null, Arg.Any<CancellationToken>()).Returns(false);
        var command = new CreateDrinkCommand("Margarita", "A classic", null, [], []);

        var id = await _handler.Handle(command, CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
        _drinkRepository.Received(1).Add(Arg.Any<Drink>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        _drinkRepository.ExistsByNameAsync("Margarita", null, Arg.Any<CancellationToken>()).Returns(true);
        var command = new CreateDrinkCommand("Margarita", null, null, [], []);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None).AsTask());
    }
}
