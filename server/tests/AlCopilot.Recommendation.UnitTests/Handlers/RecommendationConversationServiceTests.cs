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
    public void BuildChatMessages_ReplaysPersistedNativeHistoryInSequenceOrder()
    {
        var session = ChatSession.Create("customer-1", "First request");
        var messages = RecommendationChatHistoryProvider.BuildChatMessages(
            [
                CreateAgentMessage(session.Id, 4, ChatRole.Assistant, "Second answer"),
                CreateAgentMessage(session.Id, 3, ChatRole.User, "Second request"),
                CreateAgentMessage(session.Id, 2, ChatRole.Assistant, "First answer"),
                CreateAgentMessage(session.Id, 1, ChatRole.User, "First request"),
            ]);

        messages.Select(message => (message.Role, message.Text)).ShouldBe(
        [
            (ChatRole.User, "First request"),
            (ChatRole.Assistant, "First answer"),
            (ChatRole.User, "Second request"),
            (ChatRole.Assistant, "Second answer"),
        ]);
    }

    [Fact]
    public void BuildChatMessages_ReplaysPersistedFollowUpTurnHistory()
    {
        var session = ChatSession.Create("customer-1", "Can I make a Dark 'n' Stormy?");
        var messages = RecommendationChatHistoryProvider.BuildChatMessages(
            [
                CreateAgentMessage(session.Id, 1, ChatRole.User, "Can I make a Dark 'n' Stormy?"),
                CreateAgentMessage(session.Id, 2, ChatRole.Assistant, "Yes, you can already make a Dark 'n' Stormy."),
                CreateAgentMessage(
                    session.Id,
                    3,
                    ChatRole.User,
                    "Oh I see that I can already make Dark 'n' stormy from what I have. Any other drinks I can already make or at least close to that?"),
            ]);

        messages.Select(message => (message.Role, message.Text)).ShouldBe(
        [
            (ChatRole.User, "Can I make a Dark 'n' Stormy?"),
            (ChatRole.Assistant, "Yes, you can already make a Dark 'n' Stormy."),
            (ChatRole.User, "Oh I see that I can already make Dark 'n' stormy from what I have. Any other drinks I can already make or at least close to that?"),
        ]);
    }

    [Fact]
    public void BuildChatMessages_ReplaysPersistedHistoryWhenContentTypeMetadataIsNotFirst()
    {
        var session = ChatSession.Create("customer-1", "First request");
        var assistantMessage = new ChatMessage(ChatRole.Assistant, "Yes, you can already make a Dark 'n' Stormy.")
        {
            MessageId = "assistant-message",
        };
        var rawMessageJson = SerializeWithContentTypeMetadataLast(assistantMessage);

        var messages = RecommendationChatHistoryProvider.BuildChatMessages(
            [
                CreateAgentMessage(session.Id, 1, ChatRole.User, "First request"),
                CreateAgentMessage(
                    session.Id,
                    null,
                    2,
                    ChatRole.Assistant,
                    assistantMessage.Text,
                    rawMessageJson,
                    assistantMessage.MessageId),
            ]);

        messages.Select(message => (message.Role, message.Text)).ShouldBe(
        [
            (ChatRole.User, "First request"),
            (ChatRole.Assistant, "Yes, you can already make a Dark 'n' Stormy."),
        ]);
    }

    [Fact]
    public async Task SendMessageAsync_CreatesSession_AndPersistsResult()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var dbContext = CreateDbContext();
        var unitOfWork = new TestUnitOfWork(dbContext);
        var agent = new FakeAgent("Try the Gimlet.", BuildRunContext());
        var agentSession = new TestAgentSession();

        agentFactory.Create(
            Arg.Any<ChatSession>(),
            Arg.Any<AgentRun>()).Returns(call =>
        {
            RecommendationRunContext? capturedRunContext = null;
            agent.Attach(
                (ChatSession)call[0]!,
                (AgentRun)call[1]!,
                value => capturedRunContext = value,
                dbContext);
            return new RecommendationNarratorAgentRuntime(
                agent,
                () => capturedRunContext,
                "ollama",
                "test-model");
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");

        var service = CreateService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            dbContext,
            unitOfWork,
            hostEnvironment,
            NullLogger<RecommendationConversationService>.Instance);

        var result = await service.SendMessageAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            CancellationToken.None);

        result.SessionId.ShouldNotBe(Guid.Empty);
        repository.Received(1).Add(Arg.Is<ChatSession>(session =>
            session.AgentSessionStateJson == """{"stateBag":{"session":"saved"}}"""));
        unitOfWork.SaveCount.ShouldBe(1);
        dbContext.AgentMessages
            .OrderBy(message => message.Sequence)
            .Select(message => message.Role)
            .ShouldBe(["user", "assistant"]);
        dbContext.RecommendationTurnGroups.Count().ShouldBe(1);
        dbContext.AgentRuns.Single().Provider.ShouldBe("ollama");
        dbContext.AgentRuns.Single().Model.ShouldBe("test-model");

        agent.SeenMessages.Select(message => (message.Role, message.Text)).ShouldBe(
        [
            (ChatRole.User, "Give me something citrusy"),
        ]);
    }

    [Fact]
    public async Task SendMessageAsync_UsesAgentRuntimeRunContext_ForRecommendationArtifacts()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var dbContext = CreateDbContext();
        var unitOfWork = new TestUnitOfWork(dbContext);
        var agent = new FakeAgent("Try the Gimlet.", BuildRunContext());
        var agentSession = new TestAgentSession();

        agentFactory.Create(
            Arg.Any<ChatSession>(),
            Arg.Any<AgentRun>()).Returns(call =>
        {
            RecommendationRunContext? capturedRunContext = null;
            agent.Attach(
                (ChatSession)call[0]!,
                (AgentRun)call[1]!,
                value => capturedRunContext = value,
                dbContext);
            return new RecommendationNarratorAgentRuntime(agent, () => capturedRunContext);
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");

        var service = CreateService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            dbContext,
            unitOfWork,
            hostEnvironment,
            NullLogger<RecommendationConversationService>.Instance);

        var result = await service.SendMessageAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            CancellationToken.None);

        result.SessionId.ShouldNotBe(Guid.Empty);
        dbContext.RecommendationTurnGroups.Count().ShouldBe(1);
        unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task SendMessageAsync_ReusesExistingSession()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var dbContext = CreateDbContext();
        var unitOfWork = new TestUnitOfWork(dbContext);
        var agent = new FakeAgent("No great matches right now.", BuildEmptyRunContext());
        var agentSession = new TestAgentSession();
        var existingSession = ChatSession.Create("customer-1", "First request");

        repository.GetByCustomerSessionIdAsync("customer-1", existingSession.Id, Arg.Any<CancellationToken>())
            .Returns(existingSession);
        agentFactory.Create(
            existingSession,
            Arg.Any<AgentRun>()).Returns(call =>
        {
            RecommendationRunContext? capturedRunContext = null;
            agent.Attach(
                (ChatSession)call[0]!,
                (AgentRun)call[1]!,
                value => capturedRunContext = value,
                dbContext);
            return new RecommendationNarratorAgentRuntime(agent, () => capturedRunContext);
        });
        sessionStore.RestoreAsync(existingSession.AgentSessionStateJson, agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"restored"}}""");

        var service = CreateService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            dbContext,
            unitOfWork,
            hostEnvironment,
            NullLogger<RecommendationConversationService>.Instance);

        var result = await service.SendMessageAsync(
            "customer-1",
            existingSession.Id,
            "Something else",
            CancellationToken.None);

        result.SessionId.ShouldBe(existingSession.Id);
        repository.DidNotReceive().Add(Arg.Any<ChatSession>());
        existingSession.AgentSessionStateJson.ShouldBe("""{"stateBag":{"session":"restored"}}""");
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
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();

        var dbContext = CreateDbContext();
        var service = CreateService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            dbContext,
            unitOfWork,
            hostEnvironment,
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
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var dbContext = CreateDbContext();
        var agent = new FakeAgent("Try the Gimlet.", BuildEmptyRunContext());
        var agentSession = new TestAgentSession();

        agentFactory.Create(
            Arg.Any<ChatSession>(),
            Arg.Any<AgentRun>()).Returns(call =>
        {
            RecommendationRunContext? capturedRunContext = null;
            agent.Attach(
                (ChatSession)call[0]!,
                (AgentRun)call[1]!,
                value => capturedRunContext = value,
                dbContext);
            return new RecommendationNarratorAgentRuntime(agent, () => capturedRunContext);
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new DbUpdateConcurrencyException("conflict"));

        var service = CreateService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            dbContext,
            unitOfWork,
            hostEnvironment,
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
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var dbContext = CreateDbContext();
        var unitOfWork = new TestUnitOfWork(dbContext);
        var agent = new FakeAgent("Try the Gimlet.", BuildEmptyRunContext());
        var agentSession = new TestAgentSession();

        hostEnvironment.EnvironmentName.Returns(Environments.Development);
        agentFactory.Create(
            Arg.Any<ChatSession>(),
            Arg.Any<AgentRun>()).Returns(call =>
        {
            RecommendationRunContext? capturedRunContext = null;
            agent.Attach(
                (ChatSession)call[0]!,
                (AgentRun)call[1]!,
                value => capturedRunContext = value,
                dbContext);
            return new RecommendationNarratorAgentRuntime(agent, () => capturedRunContext);
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");

        var service = CreateService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            dbContext,
            unitOfWork,
            hostEnvironment,
            NullLogger<RecommendationConversationService>.Instance,
            new RecommendationObservabilityOptions
            {
                PersistExecutionTraceInDevelopment = true,
            });

        var result = await service.SendMessageAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            CancellationToken.None);

        result.SessionId.ShouldNotBe(Guid.Empty);
        dbContext.AgentMessageDiagnostics.Local
            .Select(diagnostic => (diagnostic.Kind, diagnostic.Name))
            .ShouldBe([("trace", "agent.run")]);
    }

    [Fact]
    public async Task SendMessageAsync_PersistsReasoningOnAgentRunTrace_WhenProviderReturnsReasoningContent()
    {
        var repository = Substitute.For<IChatSessionRepository>();
        var agentFactory = Substitute.For<IRecommendationNarratorAgentFactory>();
        var sessionStore = Substitute.For<IRecommendationAgentSessionStore>();
        var executionTraceRecorder = new RecommendationExecutionTraceRecorder();
        var unitOfWork = Substitute.For<IRecommendationUnitOfWork>();
        var hostEnvironment = Substitute.For<IHostEnvironment>();
        var dbContext = CreateDbContext();
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
        agentFactory.Create(
            Arg.Any<ChatSession>(),
            Arg.Any<AgentRun>()).Returns(call =>
        {
            RecommendationRunContext? capturedRunContext = null;
            agent.Attach(
                (ChatSession)call[0]!,
                (AgentRun)call[1]!,
                value => capturedRunContext = value,
                dbContext);
            return new RecommendationNarratorAgentRuntime(agent, () => capturedRunContext);
        });
        sessionStore.RestoreAsync(Arg.Any<string?>(), agent, Arg.Any<CancellationToken>())
            .Returns(agentSession);
        sessionStore.SerializeAsync(agentSession, agent, Arg.Any<CancellationToken>())
            .Returns("""{"stateBag":{"session":"saved"}}""");

        var service = CreateService(
            repository,
            agentFactory,
            sessionStore,
            executionTraceRecorder,
            dbContext,
            unitOfWork,
            hostEnvironment,
            NullLogger<RecommendationConversationService>.Instance,
            new RecommendationObservabilityOptions
            {
                PersistExecutionTraceInDevelopment = true,
            });

        await service.SendMessageAsync(
            "customer-1",
            null,
            "Give me something citrusy",
            CancellationToken.None);

        dbContext.AgentMessageDiagnostics.Local
            .Select(diagnostic => (diagnostic.Kind, diagnostic.Name, diagnostic.Text))
            .ShouldContain((
                "reasoning",
                "provider.reasoning",
                "Compare the citrus-forward options before answering."));
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

    private static RecommendationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RecommendationDbContext>()
            .UseInMemoryDatabase($"recommendation-conversation-service-{Guid.NewGuid():N}")
            .Options;

        return new RecommendationDbContext(options);
    }

    private static RecommendationConversationService CreateService(
        IChatSessionRepository repository,
        IRecommendationNarratorAgentFactory agentFactory,
        IRecommendationAgentSessionStore sessionStore,
        IRecommendationExecutionTraceRecorder executionTraceRecorder,
        RecommendationDbContext dbContext,
        IRecommendationUnitOfWork unitOfWork,
        IHostEnvironment hostEnvironment,
        Microsoft.Extensions.Logging.ILogger<RecommendationConversationService> logger,
        RecommendationObservabilityOptions? observabilityOptions = null)
    {
        return new RecommendationConversationService(
            repository,
            agentFactory,
            sessionStore,
            new RecommendationAgentRunDiagnosticsRecorder(
                executionTraceRecorder,
                new AgentMessageDiagnosticRepository(dbContext),
                hostEnvironment,
                Options.Create(observabilityOptions ?? new RecommendationObservabilityOptions())),
            new AgentRunRepository(dbContext),
            new RecommendationTurnOutputRepository(dbContext),
            unitOfWork,
            logger);
    }

    private static AgentMessage CreateAgentMessage(Guid sessionId, int sequence, ChatRole role, string text) =>
        CreateAgentMessage(sessionId, null, sequence, role, text);

    private static AgentMessage CreateAgentMessage(
        Guid sessionId,
        Guid? agentRunId,
        int sequence,
        ChatRole role,
        string text) =>
        CreateAgentMessage(
            sessionId,
            agentRunId,
            sequence,
            role,
            text,
            null,
            null);

    private static AgentMessage CreateAgentMessage(
        Guid sessionId,
        Guid? agentRunId,
        int sequence,
        ChatRole role,
        string text,
        string? rawMessageJson,
        string? nativeMessageId)
    {
        var chatMessage = new ChatMessage(role, text)
        {
            MessageId = nativeMessageId ?? Guid.NewGuid().ToString("N"),
        };

        return AgentMessage.Create(
            sessionId,
            agentRunId,
            sequence,
            chatMessage.MessageId,
            role == ChatRole.Assistant ? "assistant" : "user",
            "text",
            "maf",
            text,
            rawMessageJson ?? System.Text.Json.JsonSerializer.Serialize(chatMessage, AIJsonUtilities.DefaultOptions));
    }

    private static string SerializeWithContentTypeMetadataLast(ChatMessage message)
    {
        var json = System.Text.Json.Nodes.JsonNode
            .Parse(System.Text.Json.JsonSerializer.Serialize(message, AIJsonUtilities.DefaultOptions))!
            .AsObject();
        var content = json["contents"]!.AsArray()[0]!.AsObject();
        var typeMetadata = content["$type"]!.DeepClone();

        content.Remove("$type");
        content["$type"] = typeMetadata;

        return json.ToJsonString(AIJsonUtilities.DefaultOptions);
    }

    private sealed class FakeAgent : AIAgent
    {
        private readonly AgentResponse response;
        private readonly RecommendationRunContext? runContext;
        private ChatSession? chatSession;
        private AgentRun? agentRun;
        private Action<RecommendationRunContext?>? captureRunContext;
        private RecommendationDbContext? dbContext;

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

        public void Attach(
            ChatSession session,
            AgentRun run,
            Action<RecommendationRunContext?> captureContext,
            RecommendationDbContext context)
        {
            chatSession = session;
            agentRun = run;
            captureRunContext = captureContext;
            dbContext = context;
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
            if (chatSession is not null && agentRun is not null && dbContext is not null)
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
                    var nextSequence = dbContext.AgentMessages.Local
                        .Where(message => message.ChatSessionId == chatSession.Id)
                        .Select(message => message.Sequence)
                        .DefaultIfEmpty(0)
                        .Max() + 1;
                    var userAgentMessage = CreateAgentMessage(
                        chatSession.Id,
                        agentRun.Id,
                        nextSequence,
                        ChatRole.User,
                        userMessage);
                    var assistantAgentMessage = CreateAgentMessage(
                        chatSession.Id,
                        agentRun.Id,
                        nextSequence + 1,
                        ChatRole.Assistant,
                        assistantMessage);
                    dbContext.AgentMessages.Add(userAgentMessage);
                    dbContext.AgentMessages.Add(assistantAgentMessage);
                }
            }

            captureRunContext?.Invoke(runContext);

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

    private sealed class TestUnitOfWork(RecommendationDbContext dbContext) : IRecommendationUnitOfWork
    {
        public int SaveCount { get; private set; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
