using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Domain.Aggregates;

public sealed class TagTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var tag = Tag.Create(TagName.Create("Classic"));

        tag.Id.ShouldNotBe(Guid.Empty);
        tag.Name.Value.ShouldBe("Classic");
        tag.CreatedAtUtc.ShouldNotBe(default);
    }
}

public sealed class IngredientTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var ingredient = Ingredient.Create(
            IngredientName.Create("Tequila"), ["Patron", "Don Julio"]);

        ingredient.Id.ShouldNotBe(Guid.Empty);
        ingredient.Name.Value.ShouldBe("Tequila");
        ingredient.NotableBrands.ShouldBe(["Patron", "Don Julio"]);
    }

    [Fact]
    public void Update_ReplacesNameAndBrands()
    {
        var ingredient = Ingredient.Create(
            IngredientName.Create("Vodka"), ["Absolut"]);

        ingredient.Update(IngredientName.Create("Premium Vodka"), ["Grey Goose", "Belvedere"]);

        ingredient.Name.Value.ShouldBe("Premium Vodka");
        ingredient.NotableBrands.ShouldBe(["Grey Goose", "Belvedere"]);
    }
}
