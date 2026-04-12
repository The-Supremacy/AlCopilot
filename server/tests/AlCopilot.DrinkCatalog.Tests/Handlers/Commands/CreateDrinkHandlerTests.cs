using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class CreateDrinkHandlerTests
{
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateDrinkHandler _handler;

    public CreateDrinkHandlerTests()
    {
        _handler = new CreateDrinkHandler(
            _drinkRepository,
            _tagRepository,
            new AuditLogWriter(_auditRepository),
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesDrink()
    {
        _drinkRepository.ExistsByNameAsync("Margarita", null, Arg.Any<CancellationToken>()).Returns(false);
        var command = new CreateDrinkCommand("Margarita", "IBA", "A classic", "Shake", "Salt rim", null, [], []);

        var id = await _handler.Handle(command, CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
        _drinkRepository.Received(1).Add(Arg.Any<Drink>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        _drinkRepository.ExistsByNameAsync("Margarita", null, Arg.Any<CancellationToken>()).Returns(true);
        var command = new CreateDrinkCommand("Margarita", null, null, null, null, null, [], []);

        await Should.ThrowAsync<ConflictException>(
            () => _handler.Handle(command, CancellationToken.None).AsTask());
    }
}
