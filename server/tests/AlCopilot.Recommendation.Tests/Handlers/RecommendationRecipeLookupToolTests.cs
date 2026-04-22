using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Mediator;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class RecommendationRecipeLookupToolTests
{
    [Fact]
    public async Task LookupDrinkRecipeAsync_ReturnsRecipeAndRecordsInvocation()
    {
        var mediator = Substitute.For<IMediator>();
        var recorder = new RecommendationToolInvocationRecorder();
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

        var tool = new RecommendationRecipeLookupTool(mediator, recorder);

        var result = await tool.LookupDrinkRecipeAsync(drinkId.ToString(), null);

        result.Status.ShouldBe("ok");
        result.Drink.ShouldNotBeNull();
        result.Drink.DrinkName.ShouldBe("Negroni");
        recorder.Drain().ShouldContain(invocation => invocation.ToolName == "lookup_drink_recipe");
    }

    [Fact]
    public async Task LookupDrinkRecipeAsync_ReturnsNotFoundWithoutRecordingInvocation()
    {
        var mediator = Substitute.For<IMediator>();
        var recorder = new RecommendationToolInvocationRecorder();
        mediator.Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var tool = new RecommendationRecipeLookupTool(mediator, recorder);

        var result = await tool.LookupDrinkRecipeAsync(null, "Unknown");

        result.Status.ShouldBe("not-found");
        recorder.Drain().ShouldBeEmpty();
    }
}
