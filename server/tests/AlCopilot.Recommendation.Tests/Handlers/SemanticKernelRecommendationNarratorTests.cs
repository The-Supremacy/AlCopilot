using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class AgentFrameworkRecommendationNarratorTests
{
    [Fact]
    public async Task GenerateAsync_Throws_WhenProviderConfigurationIsInvalid()
    {
        var narrationComposer = new RecommendationNarrationComposer();
        var strategyFactory = new RecommendationChatClientStrategyFactory(
            Options.Create(new RecommendationLlmOptions { Provider = RecommendationLlmOptions.OllamaProvider }),
            Options.Create(new RecommendationOllamaOptions { ModelId = string.Empty }));
        var agentFactory = new RecommendationAgentFactory(
            strategyFactory,
            NullLoggerFactory.Instance,
            new ServiceCollection().BuildServiceProvider());
        var narrator = new AgentFrameworkRecommendationNarrator(
            agentFactory,
            NullLogger<AgentFrameworkRecommendationNarrator>.Instance);

        var session = ChatSession.Create("customer-1", "Something bright with gin");
        session.AppendUserTurn("Something bright with gin");

        await Should.ThrowAsync<InvalidOperationException>(() =>
            narrator.NarrateAsync(
                new RecommendationNarrationRequest(
                    session,
                    "Something bright with gin",
                    "context"),
                CancellationToken.None));
    }

    [Fact]
    public void BuildHistoryMessages_ReplaysPriorTurns_AndBuildContextInstructionsSeparately()
    {
        var narrationComposer = new RecommendationNarrationComposer();
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000021");
        var campariId = Guid.Parse("00000000-0000-0000-0000-000000000022");
        var session = ChatSession.Create("customer-1", "Something bright with gin");
        session.AppendUserTurn("Something bright with gin");
        session.AppendAssistantTurn(
            "Try a Gimlet.",
            [new RecommendationGroupDto("make-now", "Make Now", [new RecommendationItemDto(Guid.NewGuid(), "Gimlet", null, [], [], 90)])],
            []);
        session.AppendUserTurn("What about something bitter?");

        var contextMessage = narrationComposer.BuildContextInstructions(
            "What about something bitter?",
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
            ]);

        contextMessage.ShouldNotBeNull();
        contextMessage.ShouldContain("favorites: Gin");
        contextMessage.ShouldContain("prohibited: Campari");
        contextMessage.ShouldContain("current request: What about something bitter?");
    }
}
