using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Mediator;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class RecommendationRecipeLookupToolTests
{
    [Fact]
    public async Task LookupDrinkRecipeAsync_ReturnsRecipeAndRecordsTrace()
    {
        var mediator = Substitute.For<IMediator>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var drinkId = Guid.NewGuid();
        mediator.Send(Arg.Any<GetDrinkByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(new DrinkDetailDto(
                drinkId,
                "Negroni",
                null,
                "Bittersweet and spirit-forward",
                "Stir",
                "Orange twist",
                null,
                [],
                [
                    new RecipeEntryDto(new IngredientDto(Guid.NewGuid(), "Gin", ["Plymouth"]), "1 oz", null),
                    new RecipeEntryDto(new IngredientDto(Guid.NewGuid(), "Campari", ["Campari"]), "1 oz", null),
                ]));

        var tool = new RecommendationRecipeLookupTool(mediator, executionTraceRecorder);

        var result = await tool.LookupDrinkRecipeAsync(drinkId.ToString(), null);

        result.Status.ShouldBe("ok");
        result.Drink.ShouldNotBeNull();
        result.Drink.DrinkName.ShouldBe("Negroni");
        executionTraceRecorder.Drain().ShouldContain(step => step.StepName == "tool.lookup_drink_recipe");
    }

    [Fact]
    public async Task LookupDrinkRecipeAsync_ReturnsNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        mediator.Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var tool = new RecommendationRecipeLookupTool(mediator, executionTraceRecorder);

        var result = await tool.LookupDrinkRecipeAsync(null, "Unknown");

        result.Status.ShouldBe("not-found");
    }
}
