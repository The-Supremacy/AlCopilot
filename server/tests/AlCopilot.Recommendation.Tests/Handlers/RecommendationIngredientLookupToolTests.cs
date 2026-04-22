using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Mediator;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class RecommendationIngredientLookupToolTests
{
    [Fact]
    public async Task LookupDrinksByIngredientAsync_ReturnsMatchesAndRecordsInvocation()
    {
        var mediator = Substitute.For<IMediator>();
        var recorder = new RecommendationToolInvocationRecorder();
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

        var tool = new RecommendationIngredientLookupTool(mediator, recorder, executionTraceRecorder);

        var result = await tool.LookupDrinksByIngredientAsync("Tequila");

        result.Status.ShouldBe("ok");
        result.Drinks.Select(drink => drink.DrinkName).ShouldBe(["Long Island Iced Tea", "Margarita"]);
        result.Drinks.Single(drink => drink.DrinkName == "Margarita").MatchedIngredientNames
            .ShouldBe(["Blanco Tequila"]);
        recorder.Drain().ShouldContain(invocation => invocation.ToolName == "lookup_drinks_by_ingredient");
    }

    [Fact]
    public async Task LookupDrinksByIngredientAsync_ReturnsInvalidInput_ForBlankIngredientName()
    {
        var mediator = Substitute.For<IMediator>();
        var recorder = new RecommendationToolInvocationRecorder();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var tool = new RecommendationIngredientLookupTool(mediator, recorder, executionTraceRecorder);

        var result = await tool.LookupDrinksByIngredientAsync("   ");

        result.Status.ShouldBe("invalid-input");
        result.Drinks.ShouldBeEmpty();
        await mediator.DidNotReceive().Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>());
        recorder.Drain().ShouldBeEmpty();
    }

    private static DrinkDetailDto CreateDrink(string name, params RecipeEntryDto[] entries)
    {
        return new DrinkDetailDto(Guid.NewGuid(), name, null, $"{name} description", null, null, null, [], [.. entries]);
    }

    private static RecipeEntryDto CreateRecipeEntry(string ingredientName)
    {
        return new RecipeEntryDto(new IngredientDto(Guid.NewGuid(), ingredientName, []), "1 oz", null);
    }
}
