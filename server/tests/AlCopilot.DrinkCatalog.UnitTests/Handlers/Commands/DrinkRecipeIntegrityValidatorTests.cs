using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.Shared.Errors;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.UnitTests.Handlers.Commands;

public sealed class DrinkRecipeIntegrityValidatorTests
{
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly DrinkRecipeIntegrityValidator _validator;

    public DrinkRecipeIntegrityValidatorTests()
    {
        _validator = new DrinkRecipeIntegrityValidator(_ingredientRepository);
    }

    [Fact]
    public async Task Validate_WhenAllIngredientsExist_Completes()
    {
        var ingredientId = Guid.NewGuid();
        _ingredientRepository.GetExistingIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([ingredientId]);

        await _validator.ValidateAsync(
            [new RecipeEntryInput(ingredientId, "1 oz", null)],
            CancellationToken.None);
    }

    [Fact]
    public async Task Validate_WhenIngredientIsMissing_ThrowsNotFound()
    {
        var ingredientId = Guid.NewGuid();
        _ingredientRepository.GetExistingIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _validator.ValidateAsync(
                [new RecipeEntryInput(ingredientId, "1 oz", null)],
                CancellationToken.None));

        exception.Message.ShouldContain(ingredientId.ToString());
    }
}
