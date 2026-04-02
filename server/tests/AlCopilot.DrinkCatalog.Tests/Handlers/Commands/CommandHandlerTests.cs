using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class CreateDrinkHandlerTests
{
    private readonly IDrinkRepository _drinkRepo = Substitute.For<IDrinkRepository>();
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateDrinkHandler _handler;

    public CreateDrinkHandlerTests()
    {
        _handler = new CreateDrinkHandler(_drinkRepo, _tagRepo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesDrink()
    {
        _drinkRepo.ExistsByNameAsync("Margarita", null, Arg.Any<CancellationToken>()).Returns(false);
        var command = new CreateDrinkCommand("Margarita", "A classic", null, [], []);

        var id = await _handler.Handle(command, CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
        _drinkRepo.Received(1).Add(Arg.Any<Drink>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        _drinkRepo.ExistsByNameAsync("Margarita", null, Arg.Any<CancellationToken>()).Returns(true);
        var command = new CreateDrinkCommand("Margarita", null, null, [], []);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None).AsTask());
    }
}

public sealed class UpdateDrinkHandlerTests
{
    private readonly IDrinkRepository _drinkRepo = Substitute.For<IDrinkRepository>();
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateDrinkHandler _handler;

    public UpdateDrinkHandlerTests()
    {
        _handler = new UpdateDrinkHandler(_drinkRepo, _tagRepo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFalse()
    {
        _drinkRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Drink?)null);

        var result = await _handler.Handle(
            new UpdateDrinkCommand(Guid.NewGuid(), "Test", null, null, [], []),
            CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsTrue()
    {
        var drink = Drink.Create(DrinkName.Create("Old"), null, ImageUrl.Create(null));
        _drinkRepo.GetByIdAsync(drink.Id, Arg.Any<CancellationToken>()).Returns(drink);
        _drinkRepo.ExistsByNameAsync("New", drink.Id, Arg.Any<CancellationToken>()).Returns(false);

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
        _drinkRepo.GetByIdAsync(drink.Id, Arg.Any<CancellationToken>()).Returns(drink);
        _drinkRepo.ExistsByNameAsync("Taken", drink.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(
                new UpdateDrinkCommand(drink.Id, "Taken", null, null, [], []),
                CancellationToken.None).AsTask());
    }
}

public sealed class DeleteDrinkHandlerTests
{
    private readonly IDrinkRepository _drinkRepo = Substitute.For<IDrinkRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteDrinkHandler _handler;

    public DeleteDrinkHandlerTests()
    {
        _handler = new DeleteDrinkHandler(_drinkRepo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenFound_SoftDeletesAndReturnsTrue()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));
        _drinkRepo.GetByIdAsync(drink.Id, Arg.Any<CancellationToken>()).Returns(drink);

        var result = await _handler.Handle(new DeleteDrinkCommand(drink.Id), CancellationToken.None);

        result.ShouldBeTrue();
        drink.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFalse()
    {
        _drinkRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Drink?)null);

        var result = await _handler.Handle(new DeleteDrinkCommand(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeFalse();
    }
}

public sealed class CreateTagHandlerTests
{
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateTagHandler _handler;

    public CreateTagHandlerTests()
    {
        _handler = new CreateTagHandler(_tagRepo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesTag()
    {
        _tagRepo.ExistsByNameAsync("Classic", Arg.Any<CancellationToken>()).Returns(false);

        var id = await _handler.Handle(new CreateTagCommand("Classic"), CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
        _tagRepo.Received(1).Add(Arg.Any<Tag>());
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        _tagRepo.ExistsByNameAsync("Classic", Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(new CreateTagCommand("Classic"), CancellationToken.None).AsTask());
    }
}

public sealed class DeleteTagHandlerTests
{
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteTagHandler _handler;

    public DeleteTagHandlerTests()
    {
        _handler = new DeleteTagHandler(_tagRepo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_UnreferencedTag_DeletesAndReturnsTrue()
    {
        var tag = Tag.Create(TagName.Create("Old"));
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _tagRepo.IsReferencedByDrinksAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new DeleteTagCommand(tag.Id), CancellationToken.None);

        result.ShouldBeTrue();
        _tagRepo.Received(1).Remove(tag);
    }

    [Fact]
    public async Task Handle_ReferencedTag_Throws()
    {
        var tag = Tag.Create(TagName.Create("Active"));
        _tagRepo.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _tagRepo.IsReferencedByDrinksAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(new DeleteTagCommand(tag.Id), CancellationToken.None).AsTask());
    }
}

public sealed class CreateIngredientCategoryHandlerTests
{
    private readonly IIngredientCategoryRepository _repo = Substitute.For<IIngredientCategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateIngredientCategoryHandler _handler;

    public CreateIngredientCategoryHandlerTests()
    {
        _handler = new CreateIngredientCategoryHandler(_repo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesCategory()
    {
        _repo.ExistsByNameAsync("Spirits", Arg.Any<CancellationToken>()).Returns(false);

        var id = await _handler.Handle(new CreateIngredientCategoryCommand("Spirits"), CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        _repo.ExistsByNameAsync("Spirits", Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(new CreateIngredientCategoryCommand("Spirits"), CancellationToken.None).AsTask());
    }
}

public sealed class CreateIngredientHandlerTests
{
    private readonly IIngredientRepository _ingredientRepo = Substitute.For<IIngredientRepository>();
    private readonly IIngredientCategoryRepository _categoryRepo = Substitute.For<IIngredientCategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateIngredientHandler _handler;

    public CreateIngredientHandlerTests()
    {
        _handler = new CreateIngredientHandler(_ingredientRepo, _categoryRepo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesIngredient()
    {
        var categoryId = Guid.NewGuid();
        var category = IngredientCategory.Create(CategoryName.Create("Spirits"));
        _ingredientRepo.ExistsByNameAsync("Tequila", Arg.Any<CancellationToken>()).Returns(false);
        _categoryRepo.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns(category);

        var id = await _handler.Handle(
            new CreateIngredientCommand("Tequila", categoryId, ["Patron"]), CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_Throws()
    {
        _ingredientRepo.ExistsByNameAsync("Tequila", Arg.Any<CancellationToken>()).Returns(false);
        _categoryRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IngredientCategory?)null);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(
                new CreateIngredientCommand("Tequila", Guid.NewGuid(), []), CancellationToken.None).AsTask());
    }
}

public sealed class UpdateIngredientHandlerTests
{
    private readonly IIngredientRepository _repo = Substitute.For<IIngredientRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateIngredientHandler _handler;

    public UpdateIngredientHandlerTests()
    {
        _handler = new UpdateIngredientHandler(_repo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenFound_UpdatesBrandsAndReturnsTrue()
    {
        var ingredient = Ingredient.Create(IngredientName.Create("Vodka"), Guid.NewGuid(), ["Absolut"]);
        _repo.GetByIdAsync(ingredient.Id, Arg.Any<CancellationToken>()).Returns(ingredient);

        var result = await _handler.Handle(
            new UpdateIngredientCommand(ingredient.Id, ["Grey Goose"]), CancellationToken.None);

        result.ShouldBeTrue();
        ingredient.NotableBrands.ShouldBe(["Grey Goose"]);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFalse()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Ingredient?)null);

        var result = await _handler.Handle(
            new UpdateIngredientCommand(Guid.NewGuid(), []), CancellationToken.None);

        result.ShouldBeFalse();
    }
}
