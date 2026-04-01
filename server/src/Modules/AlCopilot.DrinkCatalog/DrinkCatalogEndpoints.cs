using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.DrinkCatalog;

public static class DrinkCatalogEndpoints
{
    public static IEndpointRouteBuilder MapDrinkCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/drink-catalog");

        MapDrinkEndpoints(group);
        MapTagEndpoints(group);
        MapIngredientCategoryEndpoints(group);
        MapIngredientEndpoints(group);

        return app;
    }

    private static void MapDrinkEndpoints(RouteGroupBuilder group)
    {
        var drinks = group.MapGroup("/drinks");

        drinks.MapGet("/", async (
            [AsParameters] GetDrinksQuery query,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        });

        drinks.MapGet("/search", async (
            string q,
            int page,
            int pageSize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest("Query parameter 'q' is required and cannot be whitespace.");

            var result = await mediator.Send(new SearchDrinksQuery(q, page, pageSize), ct);
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

    private static void MapTagEndpoints(RouteGroupBuilder group)
    {
        var tags = group.MapGroup("/tags");

        tags.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetTagsQuery(), ct)));

        tags.MapPost("/", async (CreateTagCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/drink-catalog/tags/{id}", new { id });
        });

        tags.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteTagCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    private static void MapIngredientCategoryEndpoints(RouteGroupBuilder group)
    {
        var categories = group.MapGroup("/ingredient-categories");

        categories.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetIngredientCategoriesQuery(), ct)));

        categories.MapPost("/", async (CreateIngredientCategoryCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/drink-catalog/ingredient-categories/{id}", new { id });
        });
    }

    private static void MapIngredientEndpoints(RouteGroupBuilder group)
    {
        var ingredients = group.MapGroup("/ingredients");

        ingredients.MapGet("/", async (
            Guid? categoryId,
            IMediator mediator,
            CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetIngredientsQuery(categoryId), ct)));

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
    }
}
