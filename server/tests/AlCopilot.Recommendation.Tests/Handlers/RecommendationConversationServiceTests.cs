using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using AlCopilot.Shared.Errors;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class RecommendationConversationServiceTests
{
    [Fact]
    public async Task SendMessageAsync_CreatesSession_AssemblesHistory_AndPersistsResult()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var runContextQueryService = Substitute.For<IRecommendationRunContextQueryService>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var agent = new FakeAgent("Try the Gimlet.");
        var agentSession = Substitute.For<AgentSession>();

        agentFactory.Create().Returns(agent);
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");
        toolInvocationRecorder.Drain().Returns([]);
        runContextQueryService.GetRunContextAsync("Give me something citrusy", Arg.Any<CancellationToken>())
            .Returns(new RecommendationRunContext(
                new CustomerProfileDto([], [], [], []),
                [
                    new RecommendationGroupDto(
                        "make-now",
                        "Make Now",
                        [new RecommendationItemDto(Guid.NewGuid(), "Gimlet", "Bright and citrusy", [], ["citrusy"], 100)])
                ],
                new Dictionary<Guid, string>(),
                [
                    new RecommendationRunContextGroup(
                        "make-now",
                        "Make Now",
                        [
                            new RecommendationRunContextItem(
                                Guid.NewGuid(),
                                "Gimlet",
                                "Bright and citrusy",
                                ["Gin"],
                                [],
                                ["Gin"],
                                null,
                                null,
                                100)
                        ])
                ]));

        var service = new RecommendationConversationService(
            repository,
            runContextQueryService,
            agentFactory,
            sessionStore,
            toolInvocationRecorder,
            unitOfWork,
            NullLogger<RecommendationConversationService>.Instance);

        var result = await service.SendMessageAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            CancellationToken.None);

        result.Turns.Count.ShouldBe(2);
        result.Turns.First().Role.ShouldBe("user");
        result.Turns.Last().Role.ShouldBe("assistant");
        result.Turns.Last().RecommendationGroups.Count.ShouldBe(1);
        repository.Received(1).Add(Arg.Is<ChatSession>(session =>
            session.AgentSessionStateJson == """{"stateBag":{"session":"saved"}}"""));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        agent.SeenMessages.Select(message => (message.Role, message.Text)).ShouldBe(
        [
            (ChatRole.User, "Give me something citrusy"),
        ]);
    }

    [Fact]
    public async Task SendMessageAsync_ReusesExistingSession_AndPersistsRecordedToolInvocations()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var runContextQueryService = Substitute.For<IRecommendationRunContextQueryService>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var agent = new FakeAgent("No great matches right now.");
        var agentSession = Substitute.For<AgentSession>();
        var existingSession = ChatSession.Create("customer-1", "First request");
        existingSession.AppendUserTurn("First request");
        existingSession.AppendAssistantTurn("First answer", [], []);

        repository.GetByCustomerSessionIdAsync("customer-1", existingSession.Id, Arg.Any<CancellationToken>())
            .Returns(existingSession);
        agentFactory.Create().Returns(agent);
        sessionStore.RestoreAsync(existingSession.AgentSessionStateJson, agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"restored"}}""");
        toolInvocationRecorder.Drain()
            .Returns([
                new RecommendationToolInvocationDto("lookup_drink_recipe", "Looked up the full recipe details for Negroni.")
            ]);
        runContextQueryService.GetRunContextAsync("Something else", Arg.Any<CancellationToken>())
            .Returns(new RecommendationRunContext(
                new CustomerProfileDto([], [], [], []),
                [new RecommendationGroupDto("make-now", "Make Now", [])],
                new Dictionary<Guid, string>(),
                []));

        var service = new RecommendationConversationService(
            repository,
            runContextQueryService,
            agentFactory,
            sessionStore,
            toolInvocationRecorder,
            unitOfWork,
            NullLogger<RecommendationConversationService>.Instance);

        var result = await service.SendMessageAsync(
            "customer-1",
            existingSession.Id,
            "Something else",
            CancellationToken.None);

        result.SessionId.ShouldBe(existingSession.Id);
        repository.DidNotReceive().Add(Arg.Any<ChatSession>());
        existingSession.AgentSessionStateJson.ShouldBe("""{"stateBag":{"session":"restored"}}""");
        result.Turns.Last().ToolInvocations.ShouldContain(invocation => invocation.ToolName == "lookup_drink_recipe");
        agent.SeenMessages.Select(message => (message.Role, message.Text)).ShouldBe(
        [
            (ChatRole.User, "First request"),
            (ChatRole.Assistant, "First answer"),
            (ChatRole.User, "Something else"),
        ]);
    }

    [Fact]
    public async Task SendMessageAsync_Throws_WhenMessageIsBlank()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var runContextQueryService = Substitute.For<IRecommendationRunContextQueryService>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();

        var service = new RecommendationConversationService(
            repository,
            runContextQueryService,
            agentFactory,
            sessionStore,
            toolInvocationRecorder,
            unitOfWork,
            NullLogger<RecommendationConversationService>.Instance);

        await Should.ThrowAsync<AlCopilot.Shared.Errors.ValidationException>(() =>
            service.SendMessageAsync("customer-1", null, "   ", CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_ThrowsConflict_WhenSaveHitsConcurrencyException()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var runContextQueryService = Substitute.For<IRecommendationRunContextQueryService>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var agent = new FakeAgent("Try the Gimlet.");
        var agentSession = Substitute.For<AgentSession>();

        agentFactory.Create().Returns(agent);
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");
        toolInvocationRecorder.Drain().Returns([]);
        runContextQueryService.GetRunContextAsync("Give me something citrusy", Arg.Any<CancellationToken>())
            .Returns(new RecommendationRunContext(
                new CustomerProfileDto([], [], [], []),
                [new RecommendationGroupDto("make-now", "Make Now", [])],
                new Dictionary<Guid, string>(),
                []));
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new DbUpdateConcurrencyException("conflict"));

        var service = new RecommendationConversationService(
            repository,
            runContextQueryService,
            agentFactory,
            sessionStore,
            toolInvocationRecorder,
            unitOfWork,
            NullLogger<RecommendationConversationService>.Instance);

        var ex = await Should.ThrowAsync<ConflictException>(() =>
            service.SendMessageAsync("customer-1", null, "Give me something citrusy", CancellationToken.None));

        ex.Message.ShouldBe("This recommendation session was changed while your message was being processed. Please retry.");
    }

    private sealed class FakeAgent(string assistantText) : AIAgent
    {
        public List<ChatMessage> SeenMessages { get; } = [];

        protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        protected override ValueTask<System.Text.Json.JsonElement> SerializeSessionCoreAsync(
            AgentSession session,
            System.Text.Json.JsonSerializerOptions? jsonSerializerOptions = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(
            System.Text.Json.JsonElement serializedState,
            System.Text.Json.JsonSerializerOptions? jsonSerializerOptions = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        protected override Task<AgentResponse> RunCoreAsync(
            IEnumerable<ChatMessage> messages,
            AgentSession? session = null,
            AgentRunOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            SeenMessages.Clear();
            SeenMessages.AddRange(messages);
            return Task.FromResult(new AgentResponse(
                [
                    new ChatMessage(ChatRole.Assistant, assistantText),
                ]));
        }

        protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
            IEnumerable<ChatMessage> messages,
            AgentSession? session = null,
            AgentRunOptions? options = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
