using AlCopilot.Recommendation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.Recommendation;

public static class RecommendationEndpoints
{
    public static IEndpointRouteBuilder MapRecommendationEndpoints(
        this IEndpointRouteBuilder app,
        string authorizationPolicy)
    {
        var group = app.MapGroup("/api/customer/recommendations")
            .RequireAuthorization(authorizationPolicy);

        RecommendationSessionEndpoints.Map(group);

        return app;
    }
}
