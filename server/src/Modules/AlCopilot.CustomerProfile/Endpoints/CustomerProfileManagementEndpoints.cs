using AlCopilot.CustomerProfile.Contracts.Commands;
using AlCopilot.CustomerProfile.Contracts.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.CustomerProfile.Endpoints;

internal static class CustomerProfileManagementEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetCustomerProfileQuery(), ct)));

        group.MapPut("/", async (
            SaveCustomerProfileCommand command,
            IMediator mediator,
            CancellationToken ct) =>
            Results.Ok(await mediator.Send(command, ct)));
    }
}
