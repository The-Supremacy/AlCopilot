using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class RecommendationCandidateBuilderTests
{
    private readonly DeterministicRecommendationCandidateBuilder _builder = new();
    private static readonly RecommendationRequestIntent DefaultIntent =
        new(RecommendationRequestIntentKind.Recommendation, null, [], []);

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
            [],
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
    public void Build_PrioritizesRequestedIngredientMatches()
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
            RecommendationRequestIntentKind.Recommendation,
            null,
            ["Tequila"],
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
    public void Build_KeepsDrinkDetailsCandidateEvenWhenDrinkContainsProhibitedIngredient()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000045");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000046");
        var profile = new CustomerProfileDto([], [], [campariId], [ginId]);
        var drinks = new List<DrinkDetailDto>
        {
            CreateDrink("Negroni", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(campariId, "Campari")]),
            CreateDrink("Martini", [CreateRecipeEntry(ginId, "Gin")]),
        };
        var intent = new RecommendationRequestIntent(
            RecommendationRequestIntentKind.DrinkDetails,
            "Negroni",
            [],
            [],
            true);

        var groups = _builder.Build(
            "How do I make a Negroni?",
            intent,
            profile,
            drinks,
            RecommendationSemanticSearchResult.Empty);

        groups.Single().Key.ShouldBe("drink-details");
        groups.Single().Label.ShouldBe("Drink Details");
        groups.SelectMany(group => group.Items).Select(item => item.DrinkName)
            .ShouldContain("Negroni");
    }

    [Fact]
    public void Build_RequiresAllRequestedIngredientsWhenExactMatchesExist()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000047");
        var limeId = Guid.Parse("00000000-0000-0000-0000-000000000048");
        var rumId = Guid.Parse("00000000-0000-0000-0000-000000000049");

        var profile = new CustomerProfileDto([], [], [], []);
        var drinks = new List<DrinkDetailDto>
        {
            CreateDrink("Gimlet", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(limeId, "Lime")]),
            CreateDrink("Daiquiri", [CreateRecipeEntry(rumId, "Rum"), CreateRecipeEntry(limeId, "Lime")]),
            CreateDrink("Gin Rickey", [CreateRecipeEntry(ginId, "Gin")]),
        };
        var intent = new RecommendationRequestIntent(
            RecommendationRequestIntentKind.Recommendation,
            null,
            ["Gin", "Lime"],
            []);

        var groups = _builder.Build(
            "What can I make with gin and lime?",
            intent,
            profile,
            drinks,
            RecommendationSemanticSearchResult.Empty);

        groups.SelectMany(group => group.Items).Select(item => item.DrinkName)
            .ShouldContain("Gimlet");
        groups.SelectMany(group => group.Items).Select(item => item.DrinkName)
            .ShouldNotContain("Gin Rickey");
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
            new Dictionary<Guid, RecommendationSemanticSearchResult.DrinkMatch>
            {
                [drinks[0].Id] = new(
                    drinks[0].Id,
                    drinks[0].Name,
                    2.4d,
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
            new Dictionary<Guid, RecommendationSemanticSearchResult.DrinkMatch>
            {
                [negroni.Id] = new(negroni.Id, negroni.Name, 9.0d, ["bittersweet"]),
                [martini.Id] = new(martini.Id, martini.Name, 0.2d, ["classic"]),
            });

        var groups = _builder.Build("Suggest something classic", DefaultIntent, profile, [negroni, martini], semanticResult);

        groups.SelectMany(group => group.Items).Select(item => item.DrinkName).ShouldBe(["Martini"]);
    }

    [Fact]
    public void Build_ExcludesCurrentTurnIngredientConstraints()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000064");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000065");
        var bourbonId = Guid.Parse("00000000-0000-0000-0000-000000000066");
        var bittersId = Guid.Parse("00000000-0000-0000-0000-000000000067");
        var sugarId = Guid.Parse("00000000-0000-0000-0000-000000000068");

        var negroni = CreateDrink("Negroni", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(campariId, "Campari")]);
        var oldFashioned = CreateDrink("Old Fashioned", [CreateRecipeEntry(bourbonId, "Bourbon"), CreateRecipeEntry(bittersId, "Angostura Bitters"), CreateRecipeEntry(sugarId, "Sugar Cube")]);
        var profile = new CustomerProfileDto([], [], [], [ginId, campariId, bourbonId, bittersId, sugarId]);
        var intent = new RecommendationRequestIntent(
            RecommendationRequestIntentKind.Recommendation,
            null,
            [],
            [],
            false,
            ["Campari"]);

        var groups = _builder.Build("Actually, no Campari.", intent, profile, [negroni, oldFashioned], RecommendationSemanticSearchResult.Empty);

        groups.Single(group => group.Key == "make-now").Items.Select(item => item.DrinkName)
            .ShouldBe(["Old Fashioned"]);
    }

    [Fact]
    public void Build_PrefersNonDislikedCandidatesEvenWhenDislikedCandidateScoresHigher()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000081");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000082");
        var vermouthId = Guid.Parse("00000000-0000-0000-0000-000000000083");
        var bourbonId = Guid.Parse("00000000-0000-0000-0000-000000000084");
        var bittersId = Guid.Parse("00000000-0000-0000-0000-000000000085");
        var sugarId = Guid.Parse("00000000-0000-0000-0000-000000000086");

        var negroni = CreateDrink("Negroni", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(campariId, "Campari"), CreateRecipeEntry(vermouthId, "Sweet Vermouth")], "Classy, strong, and aromatic.");
        var oldFashioned = CreateDrink("Old Fashioned", [CreateRecipeEntry(bourbonId, "Bourbon"), CreateRecipeEntry(bittersId, "Angostura Bitters"), CreateRecipeEntry(sugarId, "Sugar Cube")], "Classic, strong, and aromatic.");
        var profile = new CustomerProfileDto(
            [],
            [campariId],
            [],
            [ginId, campariId, vermouthId, bourbonId, bittersId, sugarId]);
        var semanticResult = new RecommendationSemanticSearchResult(
            new Dictionary<Guid, RecommendationSemanticSearchResult.DrinkMatch>
            {
                [negroni.Id] = new(negroni.Id, negroni.Name, 9.0d, ["classy", "strong"]),
            });

        var groups = _builder.Build("I want something classy, strong, and aromatic.", DefaultIntent, profile, [negroni, oldFashioned], semanticResult);

        groups.Single(group => group.Key == "make-now").Items.Select(item => item.DrinkName)
            .ShouldBe(["Old Fashioned", "Negroni"]);
    }

    [Fact]
    public void Build_UsesDislikedCandidatesAsFallbackWhenNoNonDislikedCandidateExistsForGroup()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000091");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000092");
        var vermouthId = Guid.Parse("00000000-0000-0000-0000-000000000093");

        var negroni = CreateDrink("Negroni", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(campariId, "Campari"), CreateRecipeEntry(vermouthId, "Sweet Vermouth")], "Bittersweet and spirit-forward.");
        var profile = new CustomerProfileDto(
            [],
            [campariId],
            [],
            [ginId, campariId, vermouthId]);

        var groups = _builder.Build("I want something strong.", DefaultIntent, profile, [negroni], RecommendationSemanticSearchResult.Empty);

        groups.Single(group => group.Key == "make-now").Items.Select(item => item.DrinkName)
            .ShouldBe(["Negroni"]);
    }

    [Fact]
    public void Build_ScoresAvailableDislikedCandidateBelowCleanCandidateMissingOneIngredient()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000101");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000102");
        var vermouthId = Guid.Parse("00000000-0000-0000-0000-000000000103");

        var availableDisliked = CreateDrink("Negroni", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(campariId, "Campari")]);
        var cleanButMissingOne = CreateDrink("Martini", [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(vermouthId, "Sweet Vermouth")]);
        var profile = new CustomerProfileDto(
            [],
            [campariId],
            [],
            [ginId, campariId]);

        var groups = _builder.Build("I want something spirit-forward.", DefaultIntent, profile, [availableDisliked, cleanButMissingOne], RecommendationSemanticSearchResult.Empty);

        var makeNowScore = groups.Single(group => group.Key == "make-now").Items.Single().Score;
        var buyNextScore = groups.Single(group => group.Key == "buy-next").Items.Single().Score;
        makeNowScore.ShouldBeLessThan(buyNextScore);
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
            new Dictionary<Guid, RecommendationSemanticSearchResult.DrinkMatch>
            {
                [french75.Id] = new(french75.Id, french75.Name, 6.0d, ["sparkling", "sweet"]),
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
