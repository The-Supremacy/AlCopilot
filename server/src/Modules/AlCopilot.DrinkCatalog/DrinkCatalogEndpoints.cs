using AlCopilot.DrinkCatalog.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AlCopilot.DrinkCatalog;

public static class DrinkCatalogEndpoints
{
    public static IEndpointRouteBuilder MapDrinkCatalogEndpoints(this IEndpointRouteBuilder app, string authorizationPolicy)
    {
        var group = app.MapGroup("/api/drink-catalog")
            .RequireAuthorization(authorizationPolicy);

        DrinkCatalogDrinkEndpoints.Map(group);
        DrinkCatalogTagEndpoints.Map(group);
        DrinkCatalogIngredientEndpoints.Map(group);
        DrinkCatalogImportEndpoints.Map(group);
        DrinkCatalogAuditEndpoints.Map(group);

        return app;
    }
}
