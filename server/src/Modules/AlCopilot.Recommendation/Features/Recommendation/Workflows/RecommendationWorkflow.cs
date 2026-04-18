using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Shared.Errors;
using Mediator;
using Microsoft.Agents.AI.Workflows;

namespace AlCopilot.Recommendation.Features.Recommendation.Workflows;

internal sealed class RecommendationWorkflow(
    IChatSessionRepository chatSessionRepository,
    IRecommendationUnitOfWork unitOfWork,
    IMediator mediator,
    IRecommendationCandidateBuilder candidateBuilder,
    IRecommendationNarrationComposer narrationComposer,
    IRecommendationNarrator narrator) : IRecommendationWorkflow
{
    public async Task<RecommendationSessionDto> ExecuteAsync(
        string customerId,
        Guid? sessionId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var workflow = BuildWorkflow();
        var request = new RecommendationWorkflowRequest(
            customerId,
            sessionId,
            NormalizeMessage(message));
        var run = await InProcessExecution.RunAsync(
            workflow,
            request,
            cancellationToken: cancellationToken);

        var output = run.NewEvents
            .OfType<WorkflowOutputEvent>()
            .Select(evt => evt.Data)
            .OfType<RecommendationSessionDto>()
            .LastOrDefault();

        return output
            ?? throw new InvalidOperationException("Recommendation workflow completed without producing a session.");
    }

    private Workflow BuildWorkflow()
    {
        var initialize = new Func<
            RecommendationWorkflowRequest,
            CancellationToken,
            ValueTask<InitializedRecommendationContext>>(InitializeAsync)
            .BindAsExecutor("initialize-request");
        var buildCandidates = new Func<
            InitializedRecommendationContext,
            CancellationToken,
            ValueTask<PreparedRecommendationContext>>(BuildCandidatesAsync)
            .BindAsExecutor("build-candidates");
        var prepareAgentInput = new Func<
            PreparedRecommendationContext,
            CancellationToken,
            ValueTask<PreparedAgentNarrationContext>>(PrepareAgentInputAsync)
            .BindAsExecutor("prepare-agent-input");
        var narrate = new Func<
            PreparedAgentNarrationContext,
            CancellationToken,
            ValueTask<NarratedRecommendationContext>>(NarrateAsync)
            .BindAsExecutor("narrate-response");
        var persist = new Func<
            NarratedRecommendationContext,
            CancellationToken,
            ValueTask<RecommendationSessionDto>>(PersistAsync)
            .BindAsExecutor("persist-session");

        var builder = new WorkflowBuilder(initialize)
            .WithName("recommendation-request")
            .WithDescription("Builds deterministic recommendation candidates and persists narrated results.")
            .AddEdge(initialize, buildCandidates)
            .AddEdge(buildCandidates, prepareAgentInput)
            .AddEdge(prepareAgentInput, narrate)
            .AddEdge(narrate, persist)
            .WithOutputFrom(persist);

        return builder.Build(validateOrphans: true);
    }

    private async ValueTask<InitializedRecommendationContext> InitializeAsync(
        RecommendationWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        ChatSession session;
        if (request.SessionId.HasValue)
        {
            var existing = await chatSessionRepository.GetByCustomerSessionIdAsync(
                request.CustomerId,
                request.SessionId.Value,
                cancellationToken);

            session = existing ?? ChatSession.Create(request.CustomerId, request.Message);
            if (existing is null)
            {
                chatSessionRepository.Add(session);
            }
        }
        else
        {
            session = ChatSession.Create(request.CustomerId, request.Message);
            chatSessionRepository.Add(session);
        }

        var profile = await mediator.Send(new GetCustomerProfileQuery(), cancellationToken);
        var drinks = await mediator.Send(new GetRecommendationCatalogQuery(), cancellationToken);

        return new InitializedRecommendationContext(request, session, profile, drinks);
    }

    private ValueTask<PreparedRecommendationContext> BuildCandidatesAsync(
        InitializedRecommendationContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var groups = candidateBuilder.Build(
            context.Request.Message,
            context.Profile,
            context.Drinks);

        return ValueTask.FromResult(new PreparedRecommendationContext(
            context.Request,
            context.Session,
            context.Profile,
            context.Drinks,
            groups));
    }

    private ValueTask<PreparedAgentNarrationContext> PrepareAgentInputAsync(
        PreparedRecommendationContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var contextInstructions = narrationComposer.BuildContextInstructions(
            context.Request.Message,
            context.Profile,
            context.Groups,
            context.Drinks);
        return ValueTask.FromResult(new PreparedAgentNarrationContext(
            context.Session,
            context.Groups,
            context.Request.Message,
            contextInstructions));
    }

    private async ValueTask<NarratedRecommendationContext> NarrateAsync(
        PreparedAgentNarrationContext context,
        CancellationToken cancellationToken)
    {
        var narration = await narrator.NarrateAsync(
            new RecommendationNarrationRequest(
                context.Session,
                context.CustomerMessage,
                context.ContextInstructions),
            cancellationToken);

        return new NarratedRecommendationContext(
            context.Session,
            context.Groups,
            context.CustomerMessage,
            narration);
    }

    private async ValueTask<RecommendationSessionDto> PersistAsync(
        NarratedRecommendationContext context,
        CancellationToken cancellationToken)
    {
        context.Session.AppendUserTurn(context.CustomerMessage);
        context.Session.AppendAssistantTurn(
            context.Narration.Content,
            context.Groups,
            context.Narration.ToolInvocations);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return context.Session.ToDto();
    }

    private static string NormalizeMessage(string message)
    {
        var normalized = message.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException("Recommendation message is required.");
        }

        return normalized;
    }

    private sealed record RecommendationWorkflowRequest(string CustomerId, Guid? SessionId, string Message);

    private sealed record InitializedRecommendationContext(
        RecommendationWorkflowRequest Request,
        ChatSession Session,
        CustomerProfileDto Profile,
        IReadOnlyCollection<DrinkDetailDto> Drinks);

    private sealed record PreparedRecommendationContext(
        RecommendationWorkflowRequest Request,
        ChatSession Session,
        CustomerProfileDto Profile,
        IReadOnlyCollection<DrinkDetailDto> Drinks,
        IReadOnlyCollection<RecommendationGroupDto> Groups);

    private sealed record PreparedAgentNarrationContext(
        ChatSession Session,
        IReadOnlyCollection<RecommendationGroupDto> Groups,
        string CustomerMessage,
        string ContextInstructions);

    private sealed record NarratedRecommendationContext(
        ChatSession Session,
        IReadOnlyCollection<RecommendationGroupDto> Groups,
        string CustomerMessage,
        RecommendationNarrationResult Narration);
}
