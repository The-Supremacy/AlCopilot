using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class RecommendationCandidateBuilderTests
{
    private readonly DeterministicRecommendationCandidateBuilder _builder = new();
    private static readonly RecommendationRequestIntent DefaultIntent =
        new(RecommendationRequestIntentKind.Recommendation, null, null, []);

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

        var groups = _builder.Build(
            "I want something bright with gin",
            DefaultIntent,
            profile,
            drinks,
            RecommendationSemanticSearchResult.Empty);

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

        var groups = _builder.Build(
            "I want something bright",
            DefaultIntent,
            profile,
            drinks,
            RecommendationSemanticSearchResult.Empty);

        groups.Single(group => group.Key == "make-now").Items.Select(item => item.DrinkName)
            .ShouldBe(["Gimlet", "Daiquiri"]);
    }

    [Fact]
    public void Build_PopulatesMatchedSignalsFromResolvedIntent()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000031");
        var limeId = Guid.Parse("00000000-0000-0000-0000-000000000032");

        var profile = new CustomerProfileDto([], [], [], [ginId, limeId]);
        var drinks = new List<DrinkDetailDto>
        {
            CreateDrink("Gimlet", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(limeId, "Lime")], "Bright and citrusy"),
        };
        var intent = new RecommendationRequestIntent(
            RecommendationRequestIntentKind.Recommendation,
            null,
            null,
            ["citrusy"]);

        var groups = _builder.Build(
            "I want something bright with gin",
            intent,
            profile,
            drinks,
            RecommendationSemanticSearchResult.Empty);

        groups.Single(group => group.Key == "make-now").Items.Single().MatchedSignals
            .ShouldBe(["citrusy"]);
    }

    [Fact]
    public void Build_PrioritizesIngredientLedMatches()
    {
        var tequilaId = Guid.Parse("00000000-0000-0000-0000-000000000041");
        var vodkaId = Guid.Parse("00000000-0000-0000-0000-000000000042");

        var profile = new CustomerProfileDto([], [], [], []);
        var drinks = new List<DrinkDetailDto>
        {
            CreateDrink("Long Island Iced Tea", [CreateRecipeEntry(tequilaId, "Tequila"), CreateRecipeEntry(vodkaId, "Vodka")]),
            CreateDrink("Martini", [CreateRecipeEntry(vodkaId, "Vodka")]),
        };
        var intent = new RecommendationRequestIntent(
            RecommendationRequestIntentKind.IngredientDiscovery,
            null,
            "Tequila",
            []);

        var groups = _builder.Build(
            "Suggest me a drink with tequila",
            intent,
            profile,
            drinks,
            RecommendationSemanticSearchResult.Empty);

        groups.SelectMany(group => group.Items).Select(item => item.DrinkName)
            .ShouldContain("Long Island Iced Tea");
        groups.SelectMany(group => group.Items).Select(item => item.DrinkName)
            .ShouldNotContain("Martini");
    }

    [Fact]
    public void Build_BoostsSemanticDescriptionMatches()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000051");
        var proseccoId = Guid.Parse("00000000-0000-0000-0000-000000000052");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000053");

        var profile = new CustomerProfileDto([], [], [], []);
        var drinks = new List<DrinkDetailDto>
        {
            CreateDrink("French 75", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(proseccoId, "Prosecco")], "Sparkling, bright, and lightly sweet."),
            CreateDrink("Negroni", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(campariId, "Campari")], "Bittersweet and spirit-forward."),
        };
        var semanticResult = new RecommendationSemanticSearchResult(
            new Dictionary<Guid, RecommendationSemanticDrinkSignal>
            {
                [drinks[0].Id] = new(
                    drinks[0].Id,
                    drinks[0].Name,
                    2.4d,
                    0d,
                    0d,
                    0.92d,
                    [RecommendationSemanticFacetKind.Description],
                    [],
                    ["sparkling", "sweet"],
                    ["sparkling", "sweet"])
            });

        var groups = _builder.Build("I want a sparkly sweet drink", DefaultIntent, profile, drinks, semanticResult);

        groups.SelectMany(group => group.Items).First().DrinkName.ShouldBe("French 75");
    }

    [Fact]
    public void Build_KeepsProhibitedIngredientsExcludedEvenWhenSemanticScoreIsHighest()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000061");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000062");
        var vermouthId = Guid.Parse("00000000-0000-0000-0000-000000000063");

        var negroni = CreateDrink("Negroni", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(campariId, "Campari"), CreateRecipeEntry(vermouthId, "Sweet Vermouth")], "Bittersweet and spirit-forward.");
        var martini = CreateDrink("Martini", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(vermouthId, "Sweet Vermouth")], "Spirit-forward and aromatic.");
        var profile = new CustomerProfileDto([], [], [campariId], [ginId, vermouthId]);
        var semanticResult = new RecommendationSemanticSearchResult(
            new Dictionary<Guid, RecommendationSemanticDrinkSignal>
            {
                [negroni.Id] = new(negroni.Id, negroni.Name, 9.0d, 0d, 0d, 0.95d, [RecommendationSemanticFacetKind.Description], [], ["bittersweet"], ["bittersweet"]),
                [martini.Id] = new(martini.Id, martini.Name, 0.2d, 0d, 0d, 0.20d, [RecommendationSemanticFacetKind.Description], [], ["classic"], ["classic"]),
            });

        var groups = _builder.Build("Suggest something classic", DefaultIntent, profile, [negroni, martini], semanticResult);

        groups.SelectMany(group => group.Items).Select(item => item.DrinkName).ShouldBe(["Martini"]);
    }

    [Fact]
    public void Build_KeepsMissingIngredientCandidatesOutOfMakeNowEvenWhenSemanticScoreIsHighest()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000071");
        var limeId = Guid.Parse("00000000-0000-0000-0000-000000000072");
        var proseccoId = Guid.Parse("00000000-0000-0000-0000-000000000073");

        var gimlet = CreateDrink("Gimlet", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(limeId, "Lime")], "Bright and citrusy.");
        var french75 = CreateDrink("French 75", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(limeId, "Lime"), CreateRecipeEntry(proseccoId, "Prosecco")], "Sparkling, bright, and lightly sweet.");
        var profile = new CustomerProfileDto([], [], [], [ginId, limeId]);
        var semanticResult = new RecommendationSemanticSearchResult(
            new Dictionary<Guid, RecommendationSemanticDrinkSignal>
            {
                [french75.Id] = new(french75.Id, french75.Name, 6.0d, 0d, 0.60d, 0.88d, [RecommendationSemanticFacetKind.Description], ["Prosecco"], ["sparkling", "sweet"], ["sparkling", "sweet"]),
            });

        var groups = _builder.Build("I want a sparkly sweet drink", DefaultIntent, profile, [gimlet, french75], semanticResult);

        groups.Single(group => group.Key == "make-now").Items.Select(item => item.DrinkName).ShouldBe(["Gimlet"]);
        groups.Single(group => group.Key == "buy-next").Items.Select(item => item.DrinkName).ShouldBe(["French 75"]);
    }

    private static DrinkDetailDto CreateDrink(string name, List<RecipeEntryDto> recipeEntries, string? description = null)
    {
        return new DrinkDetailDto(Guid.NewGuid(), name, null, description ?? $"{name} description", null, null, null, [], recipeEntries);
    }

    private static RecipeEntryDto CreateRecipeEntry(Guid ingredientId, string ingredientName)
    {
        return new RecipeEntryDto(new IngredientDto(ingredientId, ingredientName, []), "1 oz", null);
    }
}
