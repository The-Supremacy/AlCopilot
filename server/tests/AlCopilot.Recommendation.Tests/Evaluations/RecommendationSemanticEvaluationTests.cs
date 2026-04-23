using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Microsoft.Extensions.Options;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Evaluations;

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
                    new Dictionary<Guid, RecommendationSemanticDrinkSignal>
                    {
                        [french75.Id] = new(
                            french75.Id,
                            french75.Name,
                            3.0d,
                            0d,
                            0.84d,
                            0.91d,
                            [RecommendationSemanticFacetKind.Description],
                            ["Prosecco"],
                            ["sparkling", "sweet"],
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
    public async Task CreateAsync_UsesSemanticNameSignalsForMisspelledRecipeLookup()
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
            prompt =>
            {
                if (!prompt.Contains("Negrnoi", StringComparison.OrdinalIgnoreCase))
                {
                    return RecommendationSemanticSearchResult.Empty;
                }

                return new RecommendationSemanticSearchResult(
                    new Dictionary<Guid, RecommendationSemanticDrinkSignal>
                    {
                        [negroni.Id] = new(
                            negroni.Id,
                            negroni.Name,
                            2.0d,
                            0.84d,
                            0d,
                            0d,
                            [RecommendationSemanticFacetKind.Name],
                            [],
                            [],
                            [negroni.Name])
                    });
            });

        var runContext = await factory.CreateAsync("How do I make a Negrnoi?", CancellationToken.None);

        runContext.Intent.Kind.ShouldBe(RecommendationRequestIntentKind.RecipeLookup);
        runContext.Intent.RequestedDrinkName.ShouldBe("Negroni");
        runContext.RecommendationGroups.SelectMany(group => group.Items).Select(item => item.DrinkName).ShouldContain("Negroni");
    }

    private static RecommendationRunContextService CreateFactory(
        CustomerProfileDto profile,
        IReadOnlyCollection<DrinkDetailDto> drinks,
        Func<string, RecommendationSemanticSearchResult> semanticResultFactory)
    {
        return new RecommendationRunContextService(
            new StubRunInputsQueryService(new RecommendationRunInputs(profile, drinks)),
            new StubSemanticSearchService(semanticResultFactory),
            new RecommendationRequestIntentResolver(
                new StubCatalogFuzzyLookupService(),
                Options.Create(new RecommendationSemanticOptions())),
            new DeterministicRecommendationCandidateBuilder(),
            new RecommendationRunContextBuilder(),
            new RecommendationExecutionTraceRecorder());
    }

    private static DrinkDetailDto CreateDrink(Guid id, string name, string description, List<RecipeEntryDto> recipeEntries)
    {
        return new DrinkDetailDto(id, name, null, description, "Shake", null, null, [], recipeEntries);
    }

    private static RecipeEntryDto CreateRecipeEntry(Guid ingredientId, string ingredientName)
    {
        return new RecipeEntryDto(new IngredientDto(ingredientId, ingredientName, []), "1 oz", null);
    }

    private sealed class StubRunInputsQueryService(RecommendationRunInputs inputs) : IRecommendationRunInputsQueryService
    {
        public Task<RecommendationRunInputs> GetRunInputsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(inputs);
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
}
