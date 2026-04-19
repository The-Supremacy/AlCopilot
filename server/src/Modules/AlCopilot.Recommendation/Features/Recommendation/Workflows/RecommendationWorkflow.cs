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
        var narrate = new Func<
            PreparedRecommendationContext,
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
            .WithOpenTelemetry()
            .AddEdge(initialize, buildCandidates)
            .AddEdge(buildCandidates, narrate)
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

    private async ValueTask<NarratedRecommendationContext> NarrateAsync(
        PreparedRecommendationContext context,
        CancellationToken cancellationToken)
    {
        var narration = await narrator.NarrateAsync(
            new RecommendationNarrationRequest(
                context.Session,
                context.Request.Message,
                context.Profile,
                context.Groups,
                context.Drinks),
            cancellationToken);

        return new NarratedRecommendationContext(
            context.Session,
            context.Groups,
            context.Request.Message,
            narration);
    }

    private async ValueTask<RecommendationSessionDto> PersistAsync(
        NarratedRecommendationContext context,
        CancellationToken cancellationToken)
    {
        context.Session.UpdateAgentSessionState(context.Narration.SerializedAgentSessionState);
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

    private sealed record NarratedRecommendationContext(
        ChatSession Session,
        IReadOnlyCollection<RecommendationGroupDto> Groups,
        string CustomerMessage,
        RecommendationNarrationResult Narration);
}
