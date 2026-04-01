using AlCopilot.DrinkCatalog.Domain.Aggregates;
using AlCopilot.DrinkCatalog.Domain.ValueObjects;
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
        var categoryId = Guid.NewGuid();
        var ingredient = Ingredient.Create(
            IngredientName.Create("Tequila"), categoryId, ["Patron", "Don Julio"]);

        ingredient.Id.ShouldNotBe(Guid.Empty);
        ingredient.Name.Value.ShouldBe("Tequila");
        ingredient.IngredientCategoryId.ShouldBe(categoryId);
        ingredient.NotableBrands.ShouldBe(["Patron", "Don Julio"]);
    }

    [Fact]
    public void UpdateBrands_ReplacesBrands()
    {
        var ingredient = Ingredient.Create(
            IngredientName.Create("Vodka"), Guid.NewGuid(), ["Absolut"]);

        ingredient.UpdateBrands(["Grey Goose", "Belvedere"]);

        ingredient.NotableBrands.ShouldBe(["Grey Goose", "Belvedere"]);
    }
}

public sealed class IngredientCategoryTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var category = IngredientCategory.Create(CategoryName.Create("Spirits"));

        category.Id.ShouldNotBe(Guid.Empty);
        category.Name.Value.ShouldBe("Spirits");
        category.CreatedAtUtc.ShouldNotBe(default);
    }
}
