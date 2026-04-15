using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class RecommendationCandidateBuilderTests
{
    private readonly DeterministicRecommendationCandidateBuilder _builder = new();

    [Fact]
    public void Build_ExcludesProhibitedAndSplitsMakeNowVsBuyNext()
    {
        var limeId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        var profile = new CustomerProfileDto([], [], [campariId], [ginId, limeId]);
        var drinks = new List<DrinkDetailDto>
        {
            CreateDrink("Gimlet", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(limeId, "Lime")]),
            CreateDrink("Negroni", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(campariId, "Campari")]),
            CreateDrink("Martini", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(Guid.NewGuid(), "Vermouth")]),
        };

        var groups = _builder.Build("I want something bright with gin", profile, drinks);

        groups.Single(group => group.Key == "make-now").Items.Select(item => item.DrinkName)
            .ShouldBe(["Gimlet"]);
        groups.Single(group => group.Key == "buy-next").Items.Select(item => item.DrinkName)
            .ShouldBe(["Martini"]);
    }

    [Fact]
    public void Build_BoostsFavoriteIngredientMatchesWithinOtherwiseValidCandidates()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000011");
        var rumId = Guid.Parse("00000000-0000-0000-0000-000000000012");
        var limeId = Guid.Parse("00000000-0000-0000-0000-000000000013");

        var profile = new CustomerProfileDto([ginId], [], [], [ginId, rumId, limeId]);
        var drinks = new List<DrinkDetailDto>
        {
            CreateDrink("Daiquiri", [CreateRecipeEntry(rumId, "Rum"), CreateRecipeEntry(limeId, "Lime")]),
            CreateDrink("Gimlet", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(limeId, "Lime")]),
        };

        var groups = _builder.Build("I want something bright", profile, drinks);

        groups.Single(group => group.Key == "make-now").Items.Select(item => item.DrinkName)
            .ShouldBe(["Gimlet", "Daiquiri"]);
    }

    [Fact]
    public void Build_DoesNotPopulateMatchedSignalsFromPromptTokens()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000031");
        var limeId = Guid.Parse("00000000-0000-0000-0000-000000000032");

        var profile = new CustomerProfileDto([], [], [], [ginId, limeId]);
        var drinks = new List<DrinkDetailDto>
        {
            CreateDrink("Gimlet", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(limeId, "Lime")]),
        };

        var groups = _builder.Build("I want something bright with gin", profile, drinks);

        groups.Single(group => group.Key == "make-now").Items.Single().MatchedSignals
            .ShouldBeEmpty();
    }

    private static DrinkDetailDto CreateDrink(string name, List<RecipeEntryDto> recipeEntries)
    {
        return new DrinkDetailDto(Guid.NewGuid(), name, null, $"{name} description", null, null, null, [], recipeEntries);
    }

    private static RecipeEntryDto CreateRecipeEntry(Guid ingredientId, string ingredientName)
    {
        return new RecipeEntryDto(new IngredientDto(ingredientId, ingredientName, []), "1 oz", null);
    }
}
