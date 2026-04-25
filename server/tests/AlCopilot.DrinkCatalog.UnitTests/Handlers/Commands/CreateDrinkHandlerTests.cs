using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.UnitTests.Handlers.Commands;

public sealed class CreateDrinkHandlerTests
{
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly IDrinkRecipeIntegrityValidator _drinkRecipeIntegrityValidator = Substitute.For<IDrinkRecipeIntegrityValidator>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly CreateDrinkHandler _handler;

    public CreateDrinkHandlerTests()
    {
        _handler = new CreateDrinkHandler(
            _drinkRepository,
            _drinkRecipeIntegrityValidator,
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

    [Fact]
    public async Task Handle_MissingIngredient_ThrowsNotFound()
    {
        var ingredientId = Guid.NewGuid();
        _drinkRepository.ExistsByNameAsync("Margarita", null, Arg.Any<CancellationToken>()).Returns(false);
        _drinkRecipeIntegrityValidator
            .When(x => x.ValidateAsync(Arg.Any<IReadOnlyCollection<RecipeEntryInput>>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new NotFoundException($"Ingredient '{ingredientId}' not found."));

        var command = new CreateDrinkCommand(
            "Margarita",
            null,
            null,
            null,
            null,
            null,
            [],
            [new RecipeEntryInput(ingredientId, "2 oz", null)]);

        var exception = await Should.ThrowAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None).AsTask());

        exception.Message.ShouldContain(ingredientId.ToString());
    }
}
