using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Mediator;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class RecommendationIngredientLookupToolTests
{
    [Fact]
    public async Task LookupDrinksByIngredientAsync_ReturnsMatchesAndRecordsTrace()
    {
        var mediator = Substitute.For<IMediator>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        mediator.Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                CreateDrink(
                    "Long Island Iced Tea",
                    CreateRecipeEntry("Tequila"),
                    CreateRecipeEntry("Vodka")),
                CreateDrink(
                    "Margarita",
                    CreateRecipeEntry("Blanco Tequila"),
                    CreateRecipeEntry("Lime Juice")),
                CreateDrink(
                    "Daiquiri",
                    CreateRecipeEntry("White Rum"),
                    CreateRecipeEntry("Lime Juice")),
            ]);

        var tool = new RecommendationIngredientLookupTool(
            mediator,
            new StubCatalogFuzzyLookupService([]),
            executionTraceRecorder);

        var result = await tool.LookupDrinksByIngredientAsync("Tequila");

        result.Status.ShouldBe("ok");
        result.Drinks.Select(drink => drink.DrinkName).ShouldBe(["Long Island Iced Tea", "Margarita"]);
        result.Drinks.Single(drink => drink.DrinkName == "Margarita").MatchedIngredientNames
            .ShouldBe(["Blanco Tequila"]);
        executionTraceRecorder.Drain().ShouldContain(step => step.StepName == "tool.lookup_drinks_by_ingredient");
    }

    [Fact]
    public async Task LookupDrinksByIngredientAsync_ReturnsInvalidInput_ForBlankIngredientName()
    {
        var mediator = Substitute.For<IMediator>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var tool = new RecommendationIngredientLookupTool(
            mediator,
            new StubCatalogFuzzyLookupService([]),
            executionTraceRecorder);

        var result = await tool.LookupDrinksByIngredientAsync("   ");

        result.Status.ShouldBe("invalid-input");
        result.Drinks.ShouldBeEmpty();
        await mediator.DidNotReceive().Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LookupDrinksByIngredientAsync_UsesFuzzyIngredientBeforeSubstringFallback()
    {
        var proseccoId = Guid.Parse("00000000-0000-0000-0000-000000000501");
        var mediator = Substitute.For<IMediator>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        mediator.Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                CreateDrink("French 75", CreateRecipeEntry("Prosecco")),
                CreateDrink("Gimlet", CreateRecipeEntry("Gin"), CreateRecipeEntry("Lime Juice")),
            ]);
        var tool = new RecommendationIngredientLookupTool(
            mediator,
            new StubCatalogFuzzyLookupService([new RecommendationFuzzyMatch(proseccoId, "Prosecco", 0.78d)]),
            executionTraceRecorder);

        var result = await tool.LookupDrinksByIngredientAsync("Prosseco");

        result.Status.ShouldBe("ok");
        result.Drinks.Select(drink => drink.DrinkName).ShouldBe(["French 75"]);
    }

    private static DrinkDetailDto CreateDrink(string name, params RecipeEntryDto[] entries)
    {
        return new DrinkDetailDto(Guid.NewGuid(), name, null, $"{name} description", null, null, null, [], [.. entries]);
    }

    private static RecipeEntryDto CreateRecipeEntry(string ingredientName)
    {
        return new RecipeEntryDto(new IngredientDto(Guid.NewGuid(), ingredientName, []), "1 oz", null);
    }

    private sealed class StubCatalogFuzzyLookupService(IReadOnlyCollection<RecommendationFuzzyMatch> ingredientMatches)
        : IRecommendationCatalogFuzzyLookupService
    {
        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindDrinkMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<RecommendationFuzzyMatch>>([]);

        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindIngredientMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
            => Task.FromResult(ingredientMatches);
    }
}
