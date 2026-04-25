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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class RecommendationConversationServiceTests
{
    [Fact]
    public async Task SendMessageAsync_CreatesSession_AndPersistsResult()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var agent = new FakeAgent("Try the Gimlet.", BuildRunContext());
        var agentSession = new TestAgentSession();

        agentFactory.Create(Arg.Any<ChatSession>(), Arg.Any<RecommendationAgentTurnState>()).Returns(call =>
        {
            agent.Attach((ChatSession)call[0]!, (RecommendationAgentTurnState)call[1]!);
            return agent;
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");
        toolInvocationRecorder.Drain().Returns([]);

        var service = new RecommendationConversationService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            toolInvocationRecorder,
            unitOfWork,
            hostEnvironment,
            Options.Create(new RecommendationObservabilityOptions()),
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
    public async Task SendMessageAsync_UsesScopedTurnState_ForRecommendationArtifacts()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        RecommendationAgentTurnState? capturedTurnState = null;
        var agent = new FakeAgent("Try the Gimlet.", BuildRunContext());
        var agentSession = new TestAgentSession();

        agentFactory.Create(Arg.Any<ChatSession>(), Arg.Any<RecommendationAgentTurnState>()).Returns(call =>
        {
            capturedTurnState = (RecommendationAgentTurnState)call[1]!;
            agent.Attach((ChatSession)call[0]!, capturedTurnState);
            return agent;
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");
        toolInvocationRecorder.Drain().Returns([]);

        var service = new RecommendationConversationService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            toolInvocationRecorder,
            unitOfWork,
            hostEnvironment,
            Options.Create(new RecommendationObservabilityOptions()),
            NullLogger<RecommendationConversationService>.Instance);

        var result = await service.SendMessageAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            CancellationToken.None);

        result.Turns.Last().RecommendationGroups.Count.ShouldBe(1);
        capturedTurnState.ShouldNotBeNull();
        capturedTurnState.RunContext.ShouldNotBeNull();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendMessageAsync_ReusesExistingSession_AndPersistsRecordedToolInvocations()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var agent = new FakeAgent("No great matches right now.", BuildEmptyRunContext());
        var agentSession = new TestAgentSession();
        var existingSession = ChatSession.Create("customer-1", "First request");
        existingSession.AppendUserTurn("First request");
        existingSession.AppendAssistantTurn("First answer", [], []);

        repository.GetByCustomerSessionIdAsync("customer-1", existingSession.Id, Arg.Any<CancellationToken>())
            .Returns(existingSession);
        agentFactory.Create(existingSession, Arg.Any<RecommendationAgentTurnState>()).Returns(call =>
        {
            agent.Attach((ChatSession)call[0]!, (RecommendationAgentTurnState)call[1]!);
            return agent;
        });
        sessionStore.RestoreAsync(existingSession.AgentSessionStateJson, agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"restored"}}""");
        toolInvocationRecorder.Drain()
            .Returns([
                new RecommendationToolInvocationDto("lookup_drink_recipe", "Looked up the full recipe details for Negroni.")
            ]);

        var service = new RecommendationConversationService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            toolInvocationRecorder,
            unitOfWork,
            hostEnvironment,
            Options.Create(new RecommendationObservabilityOptions()),
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
            (ChatRole.User, "Something else"),
        ]);
    }

    [Fact]
    public async Task SendMessageAsync_Throws_WhenMessageIsBlank()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();

        var service = new RecommendationConversationService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            toolInvocationRecorder,
            unitOfWork,
            hostEnvironment,
            Options.Create(new RecommendationObservabilityOptions()),
            NullLogger<RecommendationConversationService>.Instance);

        await Should.ThrowAsync<AlCopilot.Shared.Errors.ValidationException>(() =>
            service.SendMessageAsync("customer-1", null, "   ", CancellationToken.None));
    }

    [Fact]
    public async Task SendMessageAsync_ThrowsConflict_WhenSaveHitsConcurrencyException()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var agent = new FakeAgent("Try the Gimlet.", BuildEmptyRunContext());
        var agentSession = new TestAgentSession();

        agentFactory.Create(Arg.Any<ChatSession>(), Arg.Any<RecommendationAgentTurnState>()).Returns(call =>
        {
            agent.Attach((ChatSession)call[0]!, (RecommendationAgentTurnState)call[1]!);
            return agent;
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");
        toolInvocationRecorder.Drain().Returns([]);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new DbUpdateConcurrencyException("conflict"));

        var service = new RecommendationConversationService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            toolInvocationRecorder,
            unitOfWork,
            hostEnvironment,
            Options.Create(new RecommendationObservabilityOptions()),
            NullLogger<RecommendationConversationService>.Instance);

        var ex = await Should.ThrowAsync<ConflictException>(() =>
            service.SendMessageAsync("customer-1", null, "Give me something citrusy", CancellationToken.None));

        ex.Message.ShouldBe("This recommendation session was changed while your message was being processed. Please retry.");
    }

    [Fact]
    public async Task SendMessageAsync_PersistsExecutionTrace_WhenDevelopmentDiagnosticsAreEnabled()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var agent = new FakeAgent("Try the Gimlet.", BuildEmptyRunContext());
        var agentSession = new TestAgentSession();

        hostEnvironment.EnvironmentName.Returns(Environments.Development);
        agentFactory.Create(Arg.Any<ChatSession>(), Arg.Any<RecommendationAgentTurnState>()).Returns(call =>
        {
            agent.Attach((ChatSession)call[0]!, (RecommendationAgentTurnState)call[1]!);
            return agent;
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");
        toolInvocationRecorder.Drain().Returns([]);

        var service = new RecommendationConversationService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            toolInvocationRecorder,
            unitOfWork,
            hostEnvironment,
            Options.Create(new RecommendationObservabilityOptions
            {
                PersistExecutionTraceInDevelopment = true,
            }),
            NullLogger<RecommendationConversationService>.Instance);

        var result = await service.SendMessageAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            CancellationToken.None);

        var assistantTurn = result.Turns.Last();
        assistantTurn.Role.ShouldBe("assistant");
        repository.Received(1).Add(Arg.Is<ChatSession>(session =>
            session.Turns.Last().GetExecutionTraceSteps().Any(step => step.StepName == "agent.run")));
    }

    [Fact]
    public async Task SendMessageAsync_PersistsReasoningOnAgentRunTrace_WhenProviderReturnsReasoningContent()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var toolInvocationRecorder = Substitute.For<IRecommendationToolInvocationRecorder>();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var agent = new FakeAgent(
            new AgentResponse(
                [
                    new ChatMessage(
                        ChatRole.Assistant,
                        [
                            new TextReasoningContent("Compare the citrus-forward options before answering."),
                            new TextContent("Try the Gimlet.")
                        ])
                ]),
            BuildEmptyRunContext());
        var agentSession = new TestAgentSession();

        hostEnvironment.EnvironmentName.Returns(Environments.Development);
        agentFactory.Create(Arg.Any<ChatSession>(), Arg.Any<RecommendationAgentTurnState>()).Returns(call =>
        {
            agent.Attach((ChatSession)call[0]!, (RecommendationAgentTurnState)call[1]!);
            return agent;
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");
        toolInvocationRecorder.Drain().Returns([]);

        var service = new RecommendationConversationService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            toolInvocationRecorder,
            unitOfWork,
            hostEnvironment,
            Options.Create(new RecommendationObservabilityOptions
            {
                PersistExecutionTraceInDevelopment = true,
            }),
            NullLogger<RecommendationConversationService>.Instance);

        await service.SendMessageAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            CancellationToken.None);

        repository.Received(1).Add(Arg.Is<ChatSession>(session =>
            session.Turns.Last().GetExecutionTraceSteps().Any(step =>
                step.StepName == "agent.run"
                && step.Reasoning == "Compare the citrus-forward options before answering."
                && step.Details.Count == 0)));
    }

    private static RecommendationRunContext BuildRunContext()
    {
        return new RecommendationRunContext(
            new RecommendationRequestIntent(
                RecommendationRequestIntentKind.Recommendation,
                null,
                [],
                ["citrusy"]),
            new CustomerProfileDto([], [], [], []),
            [
                new RecommendationGroupDto(
                    "make-now",
                    "Available Now",
                    [new RecommendationItemDto(Guid.NewGuid(), "Gimlet", "Bright and citrusy", [], ["citrusy"], 100)])
            ],
            new Dictionary<Guid, string>(),
            [
                new RecommendationRunContextGroup(
                    "make-now",
                    "Available Now",
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
                            ["citrusy"],
                            [],
                            100)
                    ])
            ],
            []);
    }

    private static RecommendationRunContext BuildEmptyRunContext()
    {
        return new RecommendationRunContext(
            new RecommendationRequestIntent(
                RecommendationRequestIntentKind.Recommendation,
                null,
                [],
                []),
            new CustomerProfileDto([], [], [], []),
            [new RecommendationGroupDto("make-now", "Available Now", [])],
            new Dictionary<Guid, string>(),
            [],
            []);
    }

    private sealed class FakeAgent : AIAgent
    {
        private readonly AgentResponse response;
        private readonly RecommendationRunContext? runContext;
        private ChatSession? chatSession;
        private RecommendationAgentTurnState? turnState;

        public FakeAgent(string assistantText)
            : this(new AgentResponse([new ChatMessage(ChatRole.Assistant, assistantText)]))
        {
        }

        public FakeAgent(string assistantText, RecommendationRunContext? runContext)
            : this(new AgentResponse([new ChatMessage(ChatRole.Assistant, assistantText)]), runContext)
        {
        }

        public FakeAgent(AgentResponse response, RecommendationRunContext? runContext = null)
        {
            this.response = response;
            this.runContext = runContext;
        }

        public List<ChatMessage> SeenMessages { get; } = [];

        public void Attach(ChatSession session, RecommendationAgentTurnState state)
        {
            chatSession = session;
            turnState = state;
        }

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
            if (chatSession is not null)
            {
                var userMessage = messages
                    .Where(message => message.Role == ChatRole.User)
                    .Select(message => message.Text)
                    .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));
                var assistantMessage = response.Messages
                    .Where(message => message.Role == ChatRole.Assistant)
                    .Select(message => message.Text)
                    .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));
                if (!string.IsNullOrWhiteSpace(userMessage) && !string.IsNullOrWhiteSpace(assistantMessage))
                {
                    chatSession.AppendUserTurn(userMessage);
                    chatSession.AppendAssistantTurn(assistantMessage, [], []);
                }
            }

            if (turnState is not null)
            {
                turnState.RunContext = runContext;
            }

            return Task.FromResult(response);
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

    private sealed class TestAgentSession : AgentSession
    {
        public TestAgentSession()
            : base(new AgentSessionStateBag())
        {
        }
    }
}
