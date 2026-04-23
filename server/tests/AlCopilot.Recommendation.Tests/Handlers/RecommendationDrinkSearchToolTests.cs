using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Mediator;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class RecommendationDrinkSearchToolTests
{
    [Fact]
    public async Task SearchDrinksAsync_ReturnsMatchesAndRecordsInvocation()
    {
        var mediator = Substitute.For<IMediator>();
        var recorder = new RecommendationToolInvocationRecorder();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        mediator.Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                CreateDrink("Long Island Iced Tea", "Crowd-pleasing highball", "Tequila", "Vodka"),
                CreateDrink("Long Vodka", "Tall and simple", "Vodka"),
                CreateDrink("Margarita", "Bright and tart", "Tequila"),
            ]);

        var tool = new RecommendationDrinkSearchTool(
            mediator,
            new StubCatalogFuzzyLookupService([]),
            recorder,
            executionTraceRecorder);

        var result = await tool.SearchDrinksAsync("Long");

        result.Status.ShouldBe("ok");
        result.Drinks.Select(drink => drink.DrinkName).ShouldBe(["Long Island Iced Tea", "Long Vodka"]);
        recorder.Drain().ShouldContain(invocation => invocation.ToolName == "search_drinks");
    }

    [Fact]
    public async Task SearchDrinksAsync_UsesFuzzyLookupBeforeSubstringFallback()
    {
        var negroni = CreateDrink("Negroni", "Bitter classic", "Gin", "Campari");
        var mediator = Substitute.For<IMediator>();
        var recorder = new RecommendationToolInvocationRecorder();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        mediator.Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns([negroni, CreateDrink("Margarita", "Bright and tart", "Tequila")]);
        var tool = new RecommendationDrinkSearchTool(
            mediator,
            new StubCatalogFuzzyLookupService([new RecommendationFuzzyMatch(negroni.Id, negroni.Name, 0.74d)]),
            recorder,
            executionTraceRecorder);

        var result = await tool.SearchDrinksAsync("Negrnoi");

        result.Status.ShouldBe("ok");
        result.Drinks.Select(drink => drink.DrinkName).ShouldBe(["Negroni"]);
    }

    private static DrinkDetailDto CreateDrink(string name, string description, params string[] ingredientNames)
    {
        return new DrinkDetailDto(
            Guid.NewGuid(),
            name,
            null,
            description,
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

    private sealed class StubCatalogFuzzyLookupService(IReadOnlyCollection<RecommendationFuzzyMatch> drinkMatches)
        : IRecommendationCatalogFuzzyLookupService
    {
        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindDrinkMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
            => Task.FromResult(drinkMatches);

        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindIngredientMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<RecommendationFuzzyMatch>>([]);
    }
}
