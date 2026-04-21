using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class RecommendationNarrationServiceTests
{
    [Fact]
    public void GenerateAsync_Throws_WhenProviderConfigurationIsInvalid()
    {
        var strategyFactory = new RecommendationChatClientStrategyFactory(
            Options.Create(new RecommendationLlmOptions { Provider = RecommendationLlmOptions.OllamaProvider }),
            Options.Create(new RecommendationOllamaOptions { ModelId = string.Empty }));

        Should.Throw<InvalidOperationException>(() => strategyFactory.Create())
            .Message.ShouldContain("Recommendation Ollama model id is required.");
    }

    [Fact]
    public void BuildCurrentRecommendationSnapshot_BuildsEphemeralRecommendationContext()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000021");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000022");
        var session = ChatSession.Create("customer-1", "Something bright with gin");
        session.AppendUserTurn("Something bright with gin");
        session.AppendAssistantTurn(
            "Try a Gimlet.",
            [new RecommendationGroupDto("make-now", "Make Now", [new RecommendationItemDto(Guid.NewGuid(), "Gimlet", null, [], [], 90)])],
            []);
        session.AppendUserTurn("What about something bitter?");

        var snapshot = RecommendationNarrationMessageBuilder.BuildCurrentRecommendationSnapshot(
            new RecommendationNarrationSnapshot(
                new CustomerProfileDto([ginId], [], [campariId], [ginId]),
                [new RecommendationGroupDto("buy-next", "Buy Next", [new RecommendationItemDto(Guid.NewGuid(), "Negroni", null, ["Campari"], [], 70)])],
                [
                    new DrinkDetailDto(
                        Guid.NewGuid(),
                        "Negroni",
                        null,
                        "Bittersweet and spirit-forward",
                        null,
                        null,
                        null,
                        [],
                        [
                            new RecipeEntryDto(new IngredientDto(ginId, "Gin", []), "1 oz", null),
                            new RecipeEntryDto(new IngredientDto(campariId, "Campari", []), "1 oz", null),
                        ])
                ]));

        snapshot.ShouldContain("authoritative product context for this response only");
        snapshot.ShouldContain("favorites: Gin");
        snapshot.ShouldContain("prohibited: Campari");
        snapshot.ShouldContain("Negroni");
    }
}
