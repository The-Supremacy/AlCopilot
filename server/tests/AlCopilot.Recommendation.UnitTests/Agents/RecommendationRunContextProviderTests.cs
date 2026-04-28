using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Agents;

public sealed class RecommendationRunContextProviderTests
{
    [Fact]
    public async Task InvokingAsync_ReturnsEmptyContext_WhenNoCustomerMessageIsAvailable()
    {
        var runInputsQueryService = Substitute.For<IRecommendationRunInputsQueryService>();
        var semanticSearchService = Substitute.For<IRecommendationSemanticSearchService>();
        var provider = CreateProvider(
            runInputsQueryService: runInputsQueryService,
            semanticSearchService: semanticSearchService);

        var result = await provider.InvokingAsync(
            CreateInvokingContext(new ChatMessage(ChatRole.System, "system-only")),
            CancellationToken.None);

        result.Messages.ShouldNotBeNull();
        result.Messages.Select(message => (message.Role, message.Text)).ShouldBe(
        [
            (ChatRole.System, "system-only"),
        ]);
        await runInputsQueryService.DidNotReceive().GetRunInputsAsync(Arg.Any<CancellationToken>());
        await semanticSearchService.DidNotReceive().SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokingAsync_SkipsSemanticSearch_ForExactDrinkDetailsRequest()
    {
        var runInputs = CreateRunInputs();
        var runInputsQueryService = Substitute.For<IRecommendationRunInputsQueryService>();
        var semanticSearchService = Substitute.For<IRecommendationSemanticSearchService>();
        var capturedRunContext = default(RecommendationRunContext);

        runInputsQueryService.GetRunInputsAsync(Arg.Any<CancellationToken>())
            .Returns(runInputs);

        var provider = CreateProvider(
            runInputsQueryService: runInputsQueryService,
            semanticSearchService: semanticSearchService,
            captureRunContext: value => capturedRunContext = value);

        var result = await provider.InvokingAsync(
            CreateInvokingContext(new ChatMessage(ChatRole.User, "How do I make a Gimlet?")),
            CancellationToken.None);

        result.Messages.ShouldNotBeNull();
        var contextMessage = result.Messages.Last();
        contextMessage.Role.ShouldBe(ChatRole.System);
        contextMessage.Text.ShouldContain("Gimlet");
        capturedRunContext.ShouldNotBeNull();
        capturedRunContext!.Intent.RequestedDrinkName.ShouldBe("Gimlet");
        await semanticSearchService.DidNotReceive().SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokingAsync_UsesSemanticSearch_ForDescriptiveRecommendationRequest()
    {
        var runInputs = CreateRunInputs();
        var semanticSearchResult = new RecommendationSemanticSearchResult(
            new Dictionary<Guid, RecommendationSemanticSearchResult.DrinkMatch>
            {
                [runInputs.Drinks.Single().Id] = new(
                    runInputs.Drinks.Single().Id,
                    "Gimlet",
                    6.0d,
                    ["bright"]),
            });
        var runInputsQueryService = Substitute.For<IRecommendationRunInputsQueryService>();
        var semanticSearchService = Substitute.For<IRecommendationSemanticSearchService>();
        var requestIntentResolver = Substitute.For<IRecommendationRequestIntentResolver>();

        runInputsQueryService.GetRunInputsAsync(Arg.Any<CancellationToken>())
            .Returns(runInputs);
        semanticSearchService.SearchAsync("I want something bright and refreshing", Arg.Any<CancellationToken>())
            .Returns(semanticSearchResult);
        requestIntentResolver.ResolveAsync(
                "I want something bright and refreshing",
                runInputs,
                semanticSearchResult,
                Arg.Any<CancellationToken>())
            .Returns(new RecommendationRequestIntent(
                RecommendationRequestIntentKind.Recommendation,
                null,
                [],
                ["bright", "refreshing"]));

        var provider = CreateProvider(
            runInputsQueryService: runInputsQueryService,
            semanticSearchService: semanticSearchService,
            requestIntentResolver: requestIntentResolver);

        var result = await provider.InvokingAsync(
            CreateInvokingContext(new ChatMessage(ChatRole.User, "I want something bright and refreshing")),
            CancellationToken.None);

        result.Messages.ShouldNotBeNull();
        result.Messages.Last().Text.ShouldContain("bright");
        await semanticSearchService.Received(1)
            .SearchAsync("I want something bright and refreshing", Arg.Any<CancellationToken>());
    }

    private static RecommendationRunContextProvider CreateProvider(
        IRecommendationRunInputsQueryService? runInputsQueryService = null,
        IRecommendationSemanticSearchService? semanticSearchService = null,
        IRecommendationRequestIntentResolver? requestIntentResolver = null,
        IRecommendationCandidateBuilder? candidateBuilder = null,
        Action<RecommendationRunContext>? captureRunContext = null)
    {
        var inputs = CreateRunInputs();
        var defaultRunInputsQueryService = Substitute.For<IRecommendationRunInputsQueryService>();
        var defaultSemanticSearchService = Substitute.For<IRecommendationSemanticSearchService>();
        var defaultRequestIntentResolver = Substitute.For<IRecommendationRequestIntentResolver>();
        var defaultCandidateBuilder = Substitute.For<IRecommendationCandidateBuilder>();

        defaultRunInputsQueryService.GetRunInputsAsync(Arg.Any<CancellationToken>())
            .Returns(inputs);
        defaultSemanticSearchService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(RecommendationSemanticSearchResult.Empty);
        defaultRequestIntentResolver.ResolveAsync(
                Arg.Any<string>(),
                Arg.Any<RecommendationRunInputs>(),
                Arg.Any<RecommendationSemanticSearchResult>(),
                Arg.Any<CancellationToken>())
            .Returns(new RecommendationRequestIntent(
                RecommendationRequestIntentKind.DrinkDetails,
                "Gimlet",
                [],
                []));
        defaultCandidateBuilder.Build(
                Arg.Any<string>(),
                Arg.Any<RecommendationRequestIntent>(),
                Arg.Any<CustomerProfileDto>(),
                Arg.Any<IReadOnlyCollection<DrinkDetailDto>>(),
                Arg.Any<RecommendationSemanticSearchResult>())
            .Returns(call =>
            {
                var drink = ((IReadOnlyCollection<DrinkDetailDto>)call[3]!).Single();
                return
                [
                    new RecommendationGroupDto(
                        "make-now",
                        "Available Now",
                        [
                            new RecommendationItemDto(
                                drink.Id,
                                drink.Name,
                                drink.Description ?? string.Empty,
                                [],
                                ["bright"],
                                100),
                        ]),
                ];
            });

        return new RecommendationRunContextProvider(
            runInputsQueryService ?? defaultRunInputsQueryService,
            semanticSearchService ?? defaultSemanticSearchService,
            requestIntentResolver ?? defaultRequestIntentResolver,
            candidateBuilder ?? defaultCandidateBuilder,
            new RecommendationRunContextBuilder(),
            new RecommendationExecutionTraceRecorder(),
            captureRunContext ?? (_ => { }));
    }

    private static AIContextProvider.InvokingContext CreateInvokingContext(params ChatMessage[] messages)
    {
#pragma warning disable MAAI001
        return new AIContextProvider.InvokingContext(
            new TestAgent(),
            new TestAgentSession(),
            new AIContext
            {
                Messages = messages,
            });
#pragma warning restore MAAI001
    }

    private static RecommendationRunInputs CreateRunInputs()
    {
        var ginId = Guid.Parse("00000000-0000-0000-0000-000000000071");
        var limeId = Guid.Parse("00000000-0000-0000-0000-000000000072");
        var gimlet = new DrinkDetailDto(
            Guid.Parse("00000000-0000-0000-0000-000000000111"),
            "Gimlet",
            null,
            "Bright gin and lime.",
            "Shake",
            null,
            null,
            [],
            [
                new RecipeEntryDto(new IngredientDto(ginId, "Gin", []), "2 oz", null),
                new RecipeEntryDto(new IngredientDto(limeId, "Lime", []), "1 oz", null),
            ]);

        return new RecommendationRunInputs(
            new CustomerProfileDto([], [], [], [ginId, limeId]),
            [gimlet]);
    }

    private sealed class TestAgent : AIAgent
    {
        protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken = default)
            => new(new TestAgentSession());

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
            => throw new NotSupportedException();

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
