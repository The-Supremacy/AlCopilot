using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class RecommendationRequestIntentResolverTests
{
    private readonly RecommendationRequestIntentResolver resolver = new(
        new StubCatalogFuzzyLookupService(),
        Options.Create(new RecommendationSemanticOptions()));

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
    public async Task Resolve_UsesFuzzyMatchBeforeSemanticFallback_WhenDrinkNameIsMisspelled()
    {
        var drink = CreateDrink("Negroni", "Gin", "Campari");
        var intent = await resolver.ResolveAsync(
            "How do I make a Negrnoi?",
            new RecommendationRunInputs(new CustomerProfileDto([], [], [], []), [drink]),
            new RecommendationSemanticSearchResult(
                new Dictionary<Guid, RecommendationSemanticDrinkSignal>
                {
                    [drink.Id] = new(
                        drink.Id,
                        drink.Name,
                        1.8d,
                        0.84d,
                        0d,
                        0d,
                        [RecommendationSemanticFacetKind.Name],
                        [],
                        [],
                        [drink.Name])
                }));

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
                new Dictionary<Guid, RecommendationSemanticDrinkSignal>
                {
                    [drink.Id] = new(
                        drink.Id,
                        drink.Name,
                        2.4d,
                        0.61d,
                        0.66d,
                        0.90d,
                        [RecommendationSemanticFacetKind.Name, RecommendationSemanticFacetKind.Description],
                        ["Prosecco"],
                        ["Sparkling, bright, and lightly sweet."],
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
                new Dictionary<Guid, RecommendationSemanticDrinkSignal>
                {
                    [drink.Id] = new(
                        drink.Id,
                        drink.Name,
                        1.6d,
                        0d,
                        0d,
                        0.90d,
                        [RecommendationSemanticFacetKind.Description],
                        [],
                        ["Sparkling, bright, and lightly sweet."],
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
    public async Task Resolve_RequiresSemanticNameMatchToClearScoreThresholdAndGap()
    {
        var negroni = CreateDrink("Negroni", "Gin", "Campari");
        var boulevardier = CreateDrink("Boulevardier", "Bourbon", "Campari");
        var resolverWithStrictOptions = new RecommendationRequestIntentResolver(
            new StubCatalogFuzzyLookupService(),
            Options.Create(new RecommendationSemanticOptions
            {
                NameMatchMinScore = 0.75d,
                FacetMatchMinScoreGap = 0.05d,
            }));

        var intent = await resolverWithStrictOptions.ResolveAsync(
            "How do I make a negrony?",
            new RecommendationRunInputs(new CustomerProfileDto([], [], [], []), [negroni, boulevardier]),
            new RecommendationSemanticSearchResult(
                new Dictionary<Guid, RecommendationSemanticDrinkSignal>
                {
                    [negroni.Id] = new(negroni.Id, negroni.Name, 1.4d, 0.74d, 0d, 0d, [RecommendationSemanticFacetKind.Name], [], [], [negroni.Name]),
                    [boulevardier.Id] = new(boulevardier.Id, boulevardier.Name, 1.3d, 0.71d, 0d, 0d, [RecommendationSemanticFacetKind.Name], [], [], [boulevardier.Name]),
                }));

        intent.RequestedDrinkName.ShouldBeNull();
        intent.Kind.ShouldBe(RecommendationRequestIntentKind.DrinkDetails);
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
            => Task.FromResult<IReadOnlyCollection<RecommendationFuzzyMatch>>([]);
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
