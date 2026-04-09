using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class CreateIngredientHandlerTests
{
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IIngredientCategoryRepository _categoryRepository = Substitute.For<IIngredientCategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateIngredientHandler _handler;

    public CreateIngredientHandlerTests()
    {
        _handler = new CreateIngredientHandler(_ingredientRepository, _categoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesIngredient()
    {
        var categoryId = Guid.NewGuid();
        var category = IngredientCategory.Create(CategoryName.Create("Spirits"));
        _ingredientRepository.ExistsByNameAsync("Tequila", Arg.Any<CancellationToken>()).Returns(false);
        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns(category);

        var id = await _handler.Handle(
            new CreateIngredientCommand("Tequila", categoryId, ["Patron"]), CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_Throws()
    {
        _ingredientRepository.ExistsByNameAsync("Tequila", Arg.Any<CancellationToken>()).Returns(false);
        _categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((IngredientCategory?)null);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(
                new CreateIngredientCommand("Tequila", Guid.NewGuid(), []), CancellationToken.None).AsTask());
    }
}
