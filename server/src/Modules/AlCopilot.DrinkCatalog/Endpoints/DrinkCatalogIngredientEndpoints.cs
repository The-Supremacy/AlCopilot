using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.DrinkCatalog.Endpoints;

internal static class DrinkCatalogIngredientEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        var ingredients = group.MapGroup("/ingredients");

        ingredients.MapGet("/", async (
            IMediator mediator,
            CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetIngredientsQuery(), ct)));

        ingredients.MapPost("/", async (CreateIngredientCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/drink-catalog/ingredients/{id}", new { id });
        });

        ingredients.MapPut("/{id:guid}/brands", async (
            Guid id,
            UpdateIngredientCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var updated = await mediator.Send(command with { IngredientId = id }, ct);
            return updated ? Results.Ok() : Results.NotFound();
        });

        ingredients.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteIngredientCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}
