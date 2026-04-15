using AlCopilot.CustomerProfile.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.CustomerProfile;

public static class CustomerProfileEndpoints
{
    public static IEndpointRouteBuilder MapCustomerProfileEndpoints(
        this IEndpointRouteBuilder app,
        string authorizationPolicy)
    {
        var group = app.MapGroup("/api/customer/profile")
            .RequireAuthorization(authorizationPolicy);

        CustomerProfileManagementEndpoints.Map(group);

        return app;
    }
}
