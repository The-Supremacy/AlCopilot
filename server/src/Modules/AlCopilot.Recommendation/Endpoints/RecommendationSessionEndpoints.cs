using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Contracts.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.Recommendation.Endpoints;

internal static class RecommendationSessionEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/sessions", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetRecommendationSessionsQuery(), ct)));

        group.MapGet("/sessions/{sessionId:guid}", async (
            Guid sessionId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var session = await mediator.Send(new GetRecommendationSessionQuery(sessionId), ct);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        group.MapPost("/messages", async (
            SubmitRecommendationRequestCommand command,
            IMediator mediator,
            CancellationToken ct) =>
            Results.Ok(await mediator.Send(command, ct)));

        group.MapPost("/sessions/{sessionId:guid}/turns/{turnId:guid}/feedback", async (
            Guid sessionId,
            Guid turnId,
            SubmitRecommendationTurnFeedbackRequest request,
            IMediator mediator,
            CancellationToken ct) =>
            Results.Ok(await mediator.Send(
                new SubmitRecommendationTurnFeedbackCommand(
                    sessionId,
                    turnId,
                    request.Rating,
                    request.Comment),
                ct)));
    }

    private sealed record SubmitRecommendationTurnFeedbackRequest(
        string Rating,
        string? Comment);
}
