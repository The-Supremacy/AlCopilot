using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class RecommendationSemanticProjectionBuilderTests
{
    [Fact]
    public void Build_CreatesDescriptionPointOnly()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000201");
        var lemonId = Guid.Parse("00000000-0000-0000-0000-000000000202");
        var drink = new DrinkDetailDto(
            Guid.Parse("00000000-0000-0000-0000-000000000299"),
            "French 75",
            "Contemporary Classics",
            "Sparkling, bright, and lightly sweet.",
            "Shake",
            "Lemon twist",
            null,
            [],
            [
                CreateRecipeEntry(ginId, "Gin"),
                CreateRecipeEntry(lemonId, "Lemon juice"),
                CreateRecipeEntry(lemonId, "lemon juice"),
            ]);

        var points = RecommendationSemanticProjectionBuilder.Build([drink]);

        points.Count.ShouldBe(1);
        points.Count(point => point.FacetKind == RecommendationSemanticFacetKind.Description).ShouldBe(1);
        points.ShouldContain(point => point.FacetKind == RecommendationSemanticFacetKind.Description && point.Text == "Sparkling, bright, and lightly sweet.");
    }

    [Fact]
    public void Build_SkipsDrinkWithoutDescription()
    {
        var drink = new DrinkDetailDto(
            Guid.Parse("00000000-0000-0000-0000-000000000399"),
            "No Description",
            "Contemporary Classics",
            " ",
            "Shake",
            null,
            null,
            [],
            [CreateRecipeEntry(Guid.Parse("00000000-0000-0000-0000-000000000301"), "Gin")]);

        var points = RecommendationSemanticProjectionBuilder.Build([drink]);

        points.ShouldBeEmpty();
    }

    private static RecipeEntryDto CreateRecipeEntry(Guid ingredientId, string ingredientName)
    {
        return new RecipeEntryDto(new IngredientDto(ingredientId, ingredientName, []), "1 oz", null);
    }
}
