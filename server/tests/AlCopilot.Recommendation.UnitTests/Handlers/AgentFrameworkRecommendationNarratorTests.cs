using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

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
    public void Create_AppliesSamplingAndReasoningConfiguration()
    {
        var strategyFactory = new RecommendationChatClientStrategyFactory(
            Options.Create(new RecommendationLlmOptions
            {
                Provider = RecommendationLlmOptions.OllamaProvider,
                Sampling = new RecommendationSamplingOptions
                {
                    Temperature = 0.35f,
                    TopP = 0.85f,
                    TopK = 32,
                },
                Reasoning = new RecommendationReasoningOptions
                {
                    Enabled = true,
                    Effort = ReasoningEffort.Medium,
                    Output = ReasoningOutput.Summary,
                },
            }),
            Options.Create(new RecommendationOllamaOptions
            {
                Endpoint = "http://localhost:11434",
                ModelId = "gemma4:e4b",
            }));

        var strategy = strategyFactory.Create();

        strategy.Provider.ShouldBe(RecommendationLlmOptions.OllamaProvider);
        strategy.Model.ShouldBe("gemma4:e4b");
        strategy.ChatOptions.Temperature.ShouldBe(0.35f);
        strategy.ChatOptions.TopP.ShouldBe(0.85f);
        strategy.ChatOptions.TopK.ShouldBe(32);
        strategy.ChatOptions.Reasoning.ShouldNotBeNull();
        strategy.ChatOptions.Reasoning!.Effort.ShouldBe(ReasoningEffort.Medium);
        strategy.ChatOptions.Reasoning!.Output.ShouldBe(ReasoningOutput.Summary);
    }

    [Fact]
    public void Build_BuildsBarAwareRecommendationRunContext()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000021");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000022");

        var message = RecommendationRunContextMessageBuilder.Build(
            new RecommendationRunContext(
                new RecommendationRequestIntent(
                    RecommendationRequestIntentKind.Recommendation,
                    null,
                    ["Gin"],
                    ["citrusy"]),
                new CustomerProfileDto([ginId], [campariId], [], [ginId]),
                [new RecommendationGroupDto("buy-next", "Consider for Restock", [new RecommendationItemDto(Guid.NewGuid(), "Negroni", null, ["Campari"], [], 70)])],
                new Dictionary<Guid, string>
                {
                    [ginId] = "Gin",
                    [campariId] = "Campari",
                },
                [
                    new RecommendationRunContextGroup(
                        "buy-next",
                        "Consider for Restock",
                        [
                            new RecommendationRunContextItem(
                                Guid.NewGuid(),
                                "Negroni",
                                "Bittersweet and spirit-forward",
                                ["Gin"],
                                ["Campari"],
                                ["Campari"],
                                ["Campari", "Gin", "Sweet Vermouth"],
                                "Stir",
                                "Orange twist",
                                ["Gin", "citrusy"],
                                [],
                                70)
                        ])
                ],
                []));

        message.ShouldContain("recommendation run context");
        message.ShouldContain("owned: Gin");
        message.ShouldContain("dislikes: Campari");
        message.ShouldContain("prohibited: none");
        message.ShouldContain("kind: Recommendation");
        message.ShouldContain("Negroni");
        message.ShouldContain("owned Gin");
        message.ShouldContain("missing Campari");
        message.ShouldContain("disliked Campari");
    }
}
