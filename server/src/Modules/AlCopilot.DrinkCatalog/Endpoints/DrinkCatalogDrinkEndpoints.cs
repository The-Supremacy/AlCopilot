using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Shared.Models;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.DrinkCatalog.Endpoints;

internal static class DrinkCatalogDrinkEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        var drinks = group.MapGroup("/drinks");

        drinks.MapGet("/", async (
            [AsParameters] DrinkBrowseRequest filters,
            [AsParameters] PagedRequest paging,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var filter = new DrinkFilter(filters.Q, filters.TagIds?.ToList(), paging.Page, paging.PageSize);
            var query = new GetDrinksQuery(filter);
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        });

        drinks.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetDrinkByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        drinks.MapPost("/", async (
            CreateDrinkCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/drink-catalog/drinks/{id}", new { id });
        });

        drinks.MapPut("/{id:guid}", async (
            Guid id,
            UpdateDrinkCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var updated = await mediator.Send(command with { DrinkId = id }, ct);
            return updated ? Results.Ok() : Results.NotFound();
        });

        drinks.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteDrinkCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    private sealed class DrinkBrowseRequest
    {
        public string? Q { get; init; }

        public Guid[]? TagIds { get; init; }
    }
}
