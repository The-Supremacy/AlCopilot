using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.DrinkCatalog.Endpoints;

internal static class DrinkCatalogImportEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        var imports = group.MapGroup("/imports");

        imports.MapGet("/history", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetImportHistoryQuery(), ct)));

        imports.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var batch = await mediator.Send(new GetImportBatchByIdQuery(id), ct);
            return batch is null ? Results.NotFound() : Results.Ok(batch);
        });

        imports.MapPost("/", async (StartImportCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var batch = await mediator.Send(command, ct);
            return Results.Created($"/api/drink-catalog/imports/{batch.Id}", batch);
        });

        imports.MapPost("/{id:guid}/review", async (Guid id, IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new ReviewImportBatchCommand(id), ct)));

        imports.MapPost("/{id:guid}/cancel", async (Guid id, IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new CancelImportBatchCommand(id), ct)));

        imports.MapPost("/{id:guid}/apply", async (Guid id, IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new ApplyImportBatchCommand(id), ct)));
    }
}
