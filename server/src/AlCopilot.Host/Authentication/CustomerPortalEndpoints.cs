using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.Host.Authentication;

public static class CustomerPortalEndpoints
{
    public static IEndpointRouteBuilder MapCustomerPortalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/customer")
            .RequireAuthorization(CustomerAuthorizationPolicies.CanAccessCustomerPortal);

        group.MapGet("/access", () => Results.NoContent());
        group.MapGet("/ingredients", async (IMediator mediator, CancellationToken ct) =>
            Results.Ok(await mediator.Send(new GetIngredientsQuery(), ct)));

        return endpoints;
    }
}
