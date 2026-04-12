using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.DrinkCatalog.Endpoints;

internal static class DrinkCatalogTagEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        var tags = group.MapGroup("/tags");

        tags.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetTagsQuery(), ct)));

        tags.MapPost("/", async (CreateTagCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/drink-catalog/tags/{id}", new { id });
        });

        tags.MapPut("/{id:guid}", async (
            Guid id,
            UpdateTagCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var updated = await mediator.Send(command with { TagId = id }, ct);
            return updated ? Results.Ok() : Results.NotFound();
        });

        tags.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteTagCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
