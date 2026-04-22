using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
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
    public void Build_BuildsBarAwareRecommendationRunContext()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000021");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000022");

        var message = RecommendationRunContextMessageBuilder.Build(
            new RecommendationRunContext(
                new CustomerProfileDto([ginId], [], [campariId], [ginId]),
                [new RecommendationGroupDto("buy-next", "Buy Next", [new RecommendationItemDto(Guid.NewGuid(), "Negroni", null, ["Campari"], [], 70)])],
                new Dictionary<Guid, string>
                {
                    [ginId] = "Gin",
                    [campariId] = "Campari",
                },
                [
                    new RecommendationRunContextGroup(
                        "buy-next",
                        "Buy Next",
                        [
                            new RecommendationRunContextItem(
                                Guid.NewGuid(),
                                "Negroni",
                                "Bittersweet and spirit-forward",
                                ["Gin"],
                                ["Campari"],
                                ["Campari", "Gin", "Sweet Vermouth"],
                                "Stir",
                                "Orange twist",
                                70)
                        ])
                ]));

        message.ShouldContain("recommendation run context");
        message.ShouldContain("owned: Gin");
        message.ShouldContain("prohibited: Campari");
        message.ShouldContain("lookup_drink_recipe");
        message.ShouldContain("Negroni");
        message.ShouldContain("owned Gin");
        message.ShouldContain("missing Campari");
    }
}
