using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class AgentFrameworkRecommendationNarratorTests
{
    [Fact]
    public async Task GenerateAsync_Throws_WhenProviderConfigurationIsInvalid()
    {
        var strategyFactory = new RecommendationChatClientStrategyFactory(
            Options.Create(new RecommendationLlmOptions { Provider = RecommendationLlmOptions.OllamaProvider }),
            Options.Create(new RecommendationOllamaOptions { ModelId = string.Empty }));

        Should.Throw<InvalidOperationException>(() => strategyFactory.Create())
            .Message.ShouldContain("Recommendation Ollama model id is required.");
    }

    [Fact]
    public void CreateContext_BuildsProfileAndCandidateSummaries()
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

        var contextMessage = RecommendationNarrationMessageBuilder.CreateContext(
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
                ]));

        contextMessage.ShouldNotBeNull();
        contextMessage.ProfileSummary.ShouldContain("favorites: Gin");
        contextMessage.ProfileSummary.ShouldContain("prohibited: Campari");
        contextMessage.ProfileSummary.ShouldContain("current request: What about something bitter?");
        contextMessage.CandidateSummary.ShouldContain("Negroni");
    }

    [Fact]
    public void BuildContextMessages_EmitsExplicitSystemContext()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000031");
        var request = new RecommendationNarrationRequest(
            ChatSession.Create("customer-1", "Need something fresh"),
            "Need something fresh",
            new CustomerProfileDto([ginId], [], [], [ginId]),
            [new RecommendationGroupDto("make-now", "Make Now", [new RecommendationItemDto(Guid.NewGuid(), "Gin Rickey", "Crisp and bright", [], ["fresh"], 91)])],
            [
                new DrinkDetailDto(
                    Guid.NewGuid(),
                    "Gin Rickey",
                    null,
                    "Crisp and bright",
                    null,
                    null,
                    null,
                        [],
                        [new RecipeEntryDto(new IngredientDto(ginId, "Gin", []), "2 oz", null)])
            ]);

        var context = RecommendationNarrationMessageBuilder.CreateContext(request);
        var messages = RecommendationNarrationMessageBuilder.BuildContextMessages(context);

        messages.Count.ShouldBe(3);
        messages[0].Role.ShouldBe(ChatRole.System);
        messages[1].Role.ShouldBe(ChatRole.System);
        messages[2].Role.ShouldBe(ChatRole.System);
        messages[1].Text.ShouldContain("favorites: Gin");
        messages[2].Text.ShouldContain("Gin Rickey");
    }

    [Fact]
    public async Task RecommendationNarrationContextProvider_StoresAndReplaysContextMessages()
    {
        var provider = new RecommendationNarrationContextProvider();
        var agent = new ChatClientAgent(
            Substitute.For<IChatClient>(),
            new ChatClientAgentOptions(),
            NullLoggerFactory.Instance,
            new ServiceCollection().BuildServiceProvider());
        var session = new TestAgentSession();
        var context = new RecommendationNarrationContext(
            "Customer profile snapshot:\n- favorites: Gin",
            "Deterministic candidate groups:\n- Make Now:\n  - Gimlet");

        RecommendationNarrationContextProvider.SetContext(session, context);

#pragma warning disable MAAI001
        var invokingContext = new AIContextProvider.InvokingContext(
            agent,
            session,
            new AIContext
            {
                Messages = [new ChatMessage(ChatRole.User, "Need something fresh")],
            });
#pragma warning restore MAAI001

        var result = await provider.InvokingAsync(invokingContext, CancellationToken.None);

        var messages = result.Messages;
        messages.ShouldNotBeNull();
        var messageList = messages.ToList();
        messageList.Count.ShouldBe(4);
        messageList.Count(message => message.Role == ChatRole.System).ShouldBe(3);
        messageList.Single(message => message.Role == ChatRole.User).Text.ShouldBe("Need something fresh");
        messageList.Any(message => (message.Text ?? string.Empty).Contains("authoritative product context", StringComparison.Ordinal))
            .ShouldBeTrue();
        messageList.Any(message => (message.Text ?? string.Empty).Contains("favorites: Gin", StringComparison.Ordinal))
            .ShouldBeTrue();
        messageList.Any(message => (message.Text ?? string.Empty).Contains("Gimlet", StringComparison.Ordinal))
            .ShouldBeTrue();
        var storedState = session.StateBag.GetValue<RecommendationNarrationContextProvider.RecommendationNarrationContextState>(
            RecommendationNarrationContextProvider.RecommendationNarrationContextState.StateKey,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        storedState.ShouldNotBeNull();
        storedState.ProfileSummary.ShouldContain("favorites: Gin");
    }

    private sealed class TestAgentSession : AgentSession;
}
