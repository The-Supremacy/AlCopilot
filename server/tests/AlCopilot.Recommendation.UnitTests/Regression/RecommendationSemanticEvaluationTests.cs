using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Regression;

public sealed class RecommendationSemanticEvaluationTests
{
    [Fact]
    public async Task CreateAsync_UsesSemanticDescriptionSignalsForDescriptivePrompt()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000401");
        var lemonId = Guid.Parse("00000000-0000-0000-0000-000000000402");
        var proseccoId = Guid.Parse("00000000-0000-0000-0000-000000000403");
        var french75 = CreateDrink(
            Guid.Parse("00000000-0000-0000-0000-000000000410"),
            "French 75",
            "Sparkling, bright, and lightly sweet.",
            [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(lemonId, "Lemon"), CreateRecipeEntry(proseccoId, "Prosecco")]);
        var gimlet = CreateDrink(
            Guid.Parse("00000000-0000-0000-0000-000000000411"),
            "Gimlet",
            "Bright and citrusy.",
            [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(lemonId, "Lemon")]);

        var factory = CreateFactory(
            new CustomerProfileDto([], [], [], [ginId, lemonId, proseccoId]),
            [french75, gimlet],
            prompt =>
            {
                if (!prompt.Contains("sparkly sweet", StringComparison.OrdinalIgnoreCase))
                {
                    return RecommendationSemanticSearchResult.Empty;
                }

                return new RecommendationSemanticSearchResult(
                    new Dictionary<Guid, RecommendationSemanticSearchResult.DrinkMatch>
                    {
                        [french75.Id] = new(
                            french75.Id,
                            french75.Name,
                            3.0d,
                            ["sparkling", "sweet"])
                    });
            });

        var runContext = await factory.CreateAsync("I want a sparkly sweet drink", CancellationToken.None);
        var prompt = RecommendationRunContextMessageBuilder.Build(runContext);

        runContext.RecommendationGroups.Single(group => group.Key == "make-now").Items.First().DrinkName.ShouldBe("French 75");
        runContext.SemanticSummaryHints.ShouldContain("sparkling");
        runContext.SemanticSummaryHints.ShouldContain("sweet");
        prompt.ShouldContain("semantic hints");
        prompt.ShouldContain("sparkling");
        prompt.ShouldContain("sweet");
    }

    [Fact]
    public async Task CreateAsync_UsesFuzzyDrinkLookupForMisspelledDrinkDetails()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000421");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000422");
        var vermouthId = Guid.Parse("00000000-0000-0000-0000-000000000423");
        var negroni = CreateDrink(
            Guid.Parse("00000000-0000-0000-0000-000000000430"),
            "Negroni",
            "Bittersweet and spirit-forward.",
            [CreateRecipeEntry(ginId, "Gin"), CreateRecipeEntry(campariId, "Campari"), CreateRecipeEntry(vermouthId, "Sweet Vermouth")]);

        var factory = CreateFactory(
            new CustomerProfileDto([], [], [], [ginId, campariId, vermouthId]),
            [negroni],
            _ => RecommendationSemanticSearchResult.Empty);

        var runContext = await factory.CreateAsync("How do I make a Negrnoi?", CancellationToken.None);

        runContext.Intent.Kind.ShouldBe(RecommendationRequestIntentKind.DrinkDetails);
        runContext.Intent.RequestedDrinkName.ShouldBe("Negroni");
        runContext.RecommendationGroups.Single().Key.ShouldBe("drink-details");
        runContext.RecommendationGroups.Single().Items.Select(item => item.DrinkName).ShouldBe(["Negroni"]);
    }

    [Fact]
    public async Task CreateAsync_UsesFuzzyIngredientLookupForMisspelledIngredientRecommendation()
    {
        var tequilaId = Guid.Parse("00000000-0000-0000-0000-000000000461");
        var limeId = Guid.Parse("00000000-0000-0000-0000-000000000462");
        var margarita = CreateDrink(
            Guid.Parse("00000000-0000-0000-0000-000000000470"),
            "Margarita",
            "Bright tequila, lime, and orange liqueur.",
            [CreateRecipeEntry(tequilaId, "Tequila"), CreateRecipeEntry(limeId, "Lime")]);

        var factory = CreateFactory(
            new CustomerProfileDto([], [], [], [tequilaId, limeId]),
            [margarita],
            _ => RecommendationSemanticSearchResult.Empty);

        var runContext = await factory.CreateAsync("What can I make with tequlia?", CancellationToken.None);

        runContext.Intent.Kind.ShouldBe(RecommendationRequestIntentKind.Recommendation);
        runContext.Intent.RequestedIngredientNames.ShouldBe(["Tequila"]);
        runContext.RecommendationGroups.Single(group => group.Key == "make-now")
            .Items.Select(item => item.DrinkName)
            .ShouldContain("Margarita");
    }

    [Fact]
    public async Task CreateAsync_SkipsSemanticSearch_ForExactDrinkDetailsRequest()
    {
        var negroni = CreateDrink(
            Guid.Parse("00000000-0000-0000-0000-000000000440"),
            "Negroni",
            "Bittersweet and spirit-forward.",
            [CreateRecipeEntry(Guid.NewGuid(), "Gin"), CreateRecipeEntry(Guid.NewGuid(), "Campari")]);
        var semanticSearchService = new CountingSemanticSearchService(_ => RecommendationSemanticSearchResult.Empty);
        var factory = CreateFactory(
            new CustomerProfileDto([], [], [], []),
            [negroni],
            semanticSearchService);

        var runContext = await factory.CreateAsync("How do I make a Negroni?", CancellationToken.None);

        semanticSearchService.CallCount.ShouldBe(0);
        runContext.Intent.Kind.ShouldBe(RecommendationRequestIntentKind.DrinkDetails);
        runContext.Intent.RequestedDrinkName.ShouldBe("Negroni");
        runContext.RecommendationGroups.Single().Key.ShouldBe("drink-details");
        runContext.RecommendationGroups.Single().Items.Select(item => item.DrinkName).ShouldBe(["Negroni"]);
    }

    [Fact]
    public async Task CreateAsync_SkipsSemanticSearch_ForIngredientOnlyRecommendation()
    {
        var gimlet = CreateDrink(
            Guid.Parse("00000000-0000-0000-0000-000000000450"),
            "Gimlet",
            "Bright and citrusy.",
            [CreateRecipeEntry(Guid.NewGuid(), "Gin"), CreateRecipeEntry(Guid.NewGuid(), "Lime")]);
        var semanticSearchService = new CountingSemanticSearchService(_ => RecommendationSemanticSearchResult.Empty);
        var factory = CreateFactory(
            new CustomerProfileDto([], [], [], []),
            [gimlet],
            semanticSearchService);

        var runContext = await factory.CreateAsync("What can I make with gin and lime?", CancellationToken.None);

        semanticSearchService.CallCount.ShouldBe(0);
        runContext.Intent.Kind.ShouldBe(RecommendationRequestIntentKind.Recommendation);
        runContext.Intent.RequestedIngredientNames.ShouldBe(["Gin", "Lime"]);
    }

    private static RecommendationRunContextEvaluationHarness CreateFactory(
        CustomerProfileDto profile,
        IReadOnlyCollection<DrinkDetailDto> drinks,
        Func<string, RecommendationSemanticSearchResult> semanticResultFactory)
    {
        return CreateFactory(
            profile,
            drinks,
            new CountingSemanticSearchService(semanticResultFactory));
    }

    private static RecommendationRunContextEvaluationHarness CreateFactory(
        CustomerProfileDto profile,
        IReadOnlyCollection<DrinkDetailDto> drinks,
        IRecommendationSemanticSearchService semanticSearchService)
    {
        return new RecommendationRunContextEvaluationHarness(
            new RecommendationRunInputs(profile, drinks),
            semanticSearchService,
            new RecommendationRequestIntentResolver(
                new StubCatalogFuzzyLookupService()),
            new DeterministicRecommendationCandidateBuilder(),
            new RecommendationRunContextBuilder());
    }

    private static DrinkDetailDto CreateDrink(Guid id, string name, string description, List<RecipeEntryDto> recipeEntries)
    {
        return new DrinkDetailDto(id, name, null, description, "Shake", null, null, [], recipeEntries);
    }

    private static RecipeEntryDto CreateRecipeEntry(Guid ingredientId, string ingredientName)
    {
        return new RecipeEntryDto(new IngredientDto(ingredientId, ingredientName, []), "1 oz", null);
    }

    private sealed class StubSemanticSearchService(
        Func<string, RecommendationSemanticSearchResult> semanticResultFactory)
        : IRecommendationSemanticSearchService
    {
        public Task<RecommendationSemanticSearchResult> SearchAsync(
            string customerMessage,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(semanticResultFactory(customerMessage));
        }
    }

    private sealed class CountingSemanticSearchService(
        Func<string, RecommendationSemanticSearchResult> semanticResultFactory)
        : IRecommendationSemanticSearchService
    {
        internal int CallCount { get; private set; }

        public Task<RecommendationSemanticSearchResult> SearchAsync(
            string customerMessage,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(semanticResultFactory(customerMessage));
        }
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
}
