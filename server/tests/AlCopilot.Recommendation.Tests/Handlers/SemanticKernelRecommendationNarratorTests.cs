using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class SemanticKernelRecommendationNarratorTests
{
    [Fact]
    public async Task GenerateAsync_Throws_WhenProviderConfigurationIsInvalid()
    {
        var narrationComposer = new RecommendationNarrationComposer();
        var kernelFactory = new RecommendationKernelFactory(
            new RecommendationReadOnlyTools(),
            Options.Create(new RecommendationLlmOptions { Provider = RecommendationLlmOptions.OllamaProvider }),
            Options.Create(new RecommendationOllamaOptions { ModelId = string.Empty }));
        var narrator = new SemanticKernelRecommendationNarrator(
            kernelFactory,
            narrationComposer,
            NullLogger<SemanticKernelRecommendationNarrator>.Instance);

        var session = ChatSession.Create("customer-1", "Something bright with gin");
        session.AppendUserTurn("Something bright with gin");

        await Should.ThrowAsync<InvalidOperationException>(() =>
            narrator.GenerateAsync(
                new RecommendationNarrationRequest(
                    session,
                    "Something bright with gin",
                    new CustomerProfileDto([], [], [], []),
                    [
                        new RecommendationGroupDto(
                            "make-now",
                            "Make Now",
                            [new RecommendationItemDto(Guid.NewGuid(), "Gimlet", "Bright and citrusy", [], ["gin"], 95)])
                    ],
                    []),
                CancellationToken.None));
    }

    [Fact]
    public void BuildChatHistory_ReplaysSessionTurnsAndAddsCurrentContext()
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

        var history = narrationComposer.BuildChatHistory(
            new RecommendationNarrationRequest(
                session,
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
                ]),
            maxHistoryTurns: 8);

        history.Count.ShouldBeGreaterThanOrEqualTo(5);
        history[0].Role.ShouldBe(AuthorRole.System);
        history[1].Role.ShouldBe(AuthorRole.System);
        var contextMessage = history[1].Content;
        contextMessage.ShouldNotBeNull();
        contextMessage.ShouldContain("favorites: Gin");
        contextMessage.ShouldContain("prohibited: Campari");
        history.Any(message => message.Role == AuthorRole.Assistant && (message.Content?.Contains("Structured recommendation summary:") ?? false))
            .ShouldBeTrue();
        history.Last().Role.ShouldBe(AuthorRole.User);
        history.Last().Content.ShouldBe("What about something bitter?");
    }
}
