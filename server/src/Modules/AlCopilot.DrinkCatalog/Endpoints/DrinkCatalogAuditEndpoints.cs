using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.DrinkCatalog.Endpoints;

internal static class DrinkCatalogAuditEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/audit-log", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetAuditLogEntriesQuery(), ct)));
    }
}
