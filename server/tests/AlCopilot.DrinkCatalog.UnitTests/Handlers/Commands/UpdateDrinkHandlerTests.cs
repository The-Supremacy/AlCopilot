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

public sealed class UpdateDrinkHandlerTests
{
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly IDrinkRecipeIntegrityValidator _drinkRecipeIntegrityValidator = Substitute.For<IDrinkRecipeIntegrityValidator>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly UpdateDrinkHandler _handler;

    public UpdateDrinkHandlerTests()
    {
        _handler = new UpdateDrinkHandler(
            _drinkRepository,
            _drinkRecipeIntegrityValidator,
            _tagRepository,
            new AuditLogWriter(_auditRepository),
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFalse()
    {
        _drinkRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Drink?)null);

        var result = await _handler.Handle(
            new UpdateDrinkCommand(Guid.NewGuid(), "Test", null, null, null, null, null, [], []),
            CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsTrue()
    {
        var drink = Drink.Create(DrinkName.Create("Old"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));
        _drinkRepository.GetByIdAsync(drink.Id, Arg.Any<CancellationToken>()).Returns(drink);
        _drinkRepository.ExistsByNameAsync("New", drink.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(
            new UpdateDrinkCommand(drink.Id, "New", "Contemporary Classics", "Desc", "Stir", "Lime", null, [], []),
            CancellationToken.None);

        result.ShouldBeTrue();
        drink.Name.Value.ShouldBe("New");
        drink.Category.ShouldBe(DrinkCategory.Create("Contemporary Classics"));
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        var drink = Drink.Create(DrinkName.Create("Old"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));
        _drinkRepository.GetByIdAsync(drink.Id, Arg.Any<CancellationToken>()).Returns(drink);
        _drinkRepository.ExistsByNameAsync("Taken", drink.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<ConflictException>(
            () => _handler.Handle(
                new UpdateDrinkCommand(drink.Id, "Taken", null, null, null, null, null, [], []),
                CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_MissingIngredient_ThrowsNotFound()
    {
        var drink = Drink.Create(DrinkName.Create("Old"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));
        var ingredientId = Guid.NewGuid();
        _drinkRepository.GetByIdAsync(drink.Id, Arg.Any<CancellationToken>()).Returns(drink);
        _drinkRepository.ExistsByNameAsync("New", drink.Id, Arg.Any<CancellationToken>()).Returns(false);
        _drinkRecipeIntegrityValidator
            .When(x => x.ValidateAsync(Arg.Any<IReadOnlyCollection<RecipeEntryInput>>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new NotFoundException($"Ingredient '{ingredientId}' not found."));

        var exception = await Should.ThrowAsync<NotFoundException>(
            () => _handler.Handle(
                new UpdateDrinkCommand(
                    drink.Id,
                    "New",
                    null,
                    null,
                    null,
                    null,
                    null,
                    [],
                    [new RecipeEntryInput(ingredientId, "1 oz", null)]),
                CancellationToken.None).AsTask());

        exception.Message.ShouldContain(ingredientId.ToString());
    }
}
