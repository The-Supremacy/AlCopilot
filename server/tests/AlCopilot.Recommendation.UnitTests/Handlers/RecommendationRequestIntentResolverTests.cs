using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class RecommendationRequestIntentResolverTests
{
    private readonly RecommendationRequestIntentResolver resolver = new(
        new StubCatalogFuzzyLookupService());

    [Fact]
    public async Task Resolve_ReturnsRecommendation_WhenMessageMentionsKnownIngredient()
    {
        var intent = await resolver.ResolveAsync(
            "Suggest me a drink with tequila",
            new RecommendationRunInputs(
                new CustomerProfileDto([], [], [], []),
                [CreateDrink("Long Island Iced Tea", "Tequila", "Vodka")]),
            RecommendationSemanticSearchResult.Empty);

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.Recommendation);
        intent.RequestedIngredientName.ShouldBe("Tequila");
        intent.RequestedIngredientNames.ShouldBe(["Tequila"]);
    }

    [Fact]
    public async Task Resolve_ReturnsDrinkDetails_WhenMessageMentionsKnownDrink()
    {
        var intent = await resolver.ResolveAsync(
            "How do I make a Negroni?",
            new RecommendationRunInputs(
                new CustomerProfileDto([], [], [], []),
                [CreateDrink("Negroni", "Gin", "Campari")]),
            RecommendationSemanticSearchResult.Empty);

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.DrinkDetails);
        intent.RequestedDrinkName.ShouldBe("Negroni");
    }

    [Fact]
    public async Task Resolve_CollectsRequestDescriptors_WhenMessageUsesKnownDescriptors()
    {
        var intent = await resolver.ResolveAsync(
            "I want something sweet and sparkling",
            new RecommendationRunInputs(
                new CustomerProfileDto([], [], [], []),
                [CreateDrink("French 75", "Gin", "Champagne")]),
            RecommendationSemanticSearchResult.Empty);

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.Recommendation);
        intent.RequestDescriptors.ShouldBe(["sparkling", "sweet"]);
    }

    [Fact]
    public async Task Resolve_TreatsNegatedIngredientMentionsAsCurrentExclusions()
    {
        var intent = await resolver.ResolveAsync(
            "Actually, no Campari.",
            new RecommendationRunInputs(
                new CustomerProfileDto([], [], [], []),
                [CreateDrink("Negroni", "Gin", "Campari")]),
            RecommendationSemanticSearchResult.Empty);

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.Recommendation);
        intent.RequestedIngredientNames.ShouldBeEmpty();
        intent.CurrentExcludedIngredientNames.ShouldBe(["Campari"]);
    }

    [Fact]
    public async Task Resolve_UsesFuzzyMatch_WhenDrinkNameIsMisspelled()
    {
        var drink = CreateDrink("Negroni", "Gin", "Campari");
        var intent = await resolver.ResolveAsync(
            "How do I make a Negrnoi?",
            new RecommendationRunInputs(new CustomerProfileDto([], [], [], []), [drink]),
            RecommendationSemanticSearchResult.Empty);

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.DrinkDetails);
        intent.RequestedDrinkName.ShouldBe("Negroni");
    }

    [Fact]
    public async Task Resolve_DoesNotPromoteDescriptivePromptToDrinkDetails_FromSemanticDrinkSignal()
    {
        var drink = CreateDrink("French 75", "Gin", "Prosecco");
        var intent = await resolver.ResolveAsync(
            "I want something sparkly and sweet",
            new RecommendationRunInputs(new CustomerProfileDto([], [], [], []), [drink]),
            new RecommendationSemanticSearchResult(
                new Dictionary<Guid, RecommendationSemanticSearchResult.DrinkMatch>
                {
                    [drink.Id] = new(
                        drink.Id,
                        drink.Name,
                        2.4d,
                        [drink.Name, "Sparkling, bright, and lightly sweet."])
                }));

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.Recommendation);
        intent.RequestedDrinkName.ShouldBeNull();
        intent.RequestDescriptors.ShouldBe(["sweet"]);
    }

    [Fact]
    public async Task Resolve_DoesNotExtractRequestDescriptorsFromSemanticDescriptors()
    {
        var drink = CreateDrink("French 75", "Gin", "Prosecco");
        var intent = await resolver.ResolveAsync(
            "Recommend me a drink",
            new RecommendationRunInputs(new CustomerProfileDto([], [], [], []), [drink]),
            new RecommendationSemanticSearchResult(
                new Dictionary<Guid, RecommendationSemanticSearchResult.DrinkMatch>
                {
                    [drink.Id] = new(
                        drink.Id,
                        drink.Name,
                        1.6d,
                        ["Sparkling, bright, and lightly sweet."])
                }));

        intent.RequestDescriptors.ShouldBeEmpty();
    }

    [Fact]
    public async Task Resolve_CollectsMultipleIngredientConstraints_FromMessageSuffix()
    {
        var intent = await resolver.ResolveAsync(
            "What can I make with gin and lime?",
            new RecommendationRunInputs(
                new CustomerProfileDto([], [], [], []),
                [CreateDrink("Gimlet", "Gin", "Lime"), CreateDrink("Daiquiri", "Rum", "Lime")]),
            RecommendationSemanticSearchResult.Empty);

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.Recommendation);
        intent.RequestedIngredientNames.ShouldBe(["Gin", "Lime"]);
    }

    [Fact]
    public async Task Resolve_UsesFuzzyMatch_WhenIngredientNameIsMisspelled()
    {
        var margarita = CreateDrink("Margarita", "Tequila", "Lime");

        var intent = await resolver.ResolveAsync(
            "What can I make with tequlia?",
            new RecommendationRunInputs(new CustomerProfileDto([], [], [], []), [margarita]),
            RecommendationSemanticSearchResult.Empty);

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.Recommendation);
        intent.RequestedIngredientNames.ShouldBe(["Tequila"]);
    }

    private sealed class StubCatalogFuzzyLookupService : IRecommendationCatalogFuzzyLookupService
    {
        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindDrinkMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<RecommendationFuzzyMatch> matches =
                string.Equals(searchText, "Negrnoi", StringComparison.OrdinalIgnoreCase)
                    ? [new RecommendationFuzzyMatch(Guid.NewGuid(), "Negroni", 0.72d)]
                    : [];

            return Task.FromResult(matches);
        }

        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindIngredientMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<RecommendationFuzzyMatch> matches =
                string.Equals(searchText, "tequlia", StringComparison.OrdinalIgnoreCase)
                    ? [new RecommendationFuzzyMatch(Guid.NewGuid(), "Tequila", 0.72d)]
                    : [];

            return Task.FromResult(matches);
        }
    }

    private static DrinkDetailDto CreateDrink(string name, params string[] ingredientNames)
    {
        return new DrinkDetailDto(
            Guid.NewGuid(),
            name,
            null,
            $"{name} description",
            null,
            null,
            null,
            [],
            ingredientNames
                .Select(ingredientName => new RecipeEntryDto(
                    new IngredientDto(Guid.NewGuid(), ingredientName, []),
                    "1 oz",
                    null))
                .ToList());
    }
}
