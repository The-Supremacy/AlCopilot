using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace AlCopilot.Host.Authentication;

public static class ManagementAuthEndpoints
{
    public static IEndpointRouteBuilder MapManagementAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth/management");

        group.MapGet("/session", (ClaimsPrincipal user) => Results.Ok(CreateSession(user)))
            .AllowAnonymous();

        group.MapGet("/login", (HttpContext context, string? returnUrl) =>
            Results.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = SanitizeReturnUrl(returnUrl),
                },
                [OpenIdConnectDefaults.AuthenticationScheme]))
            .AllowAnonymous();

        group.MapPost("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        });

        return endpoints;
    }

    private static ManagementSessionDto CreateSession(ClaimsPrincipal user)
    {
        var roles = user.Claims
            .Where(claim => string.Equals(claim.Type, "roles", StringComparison.Ordinal) ||
                            string.Equals(claim.Type, ClaimTypes.Role, StringComparison.Ordinal))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        var isAuthenticated = user.Identity?.IsAuthenticated is true;
        return new ManagementSessionDto(
            isAuthenticated,
            isAuthenticated ? user.FindFirstValue("preferred_username") ?? user.Identity?.Name : null,
            roles,
            roles.Contains("admin", StringComparer.Ordinal),
            roles.Any(role => string.Equals(role, "admin", StringComparison.Ordinal) ||
                              string.Equals(role, "manager", StringComparison.Ordinal)));
    }

    private static string SanitizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) ||
            !returnUrl.StartsWith("/", StringComparison.Ordinal) ||
            returnUrl.StartsWith("//", StringComparison.Ordinal))
        {
            return "/";
        }

        return returnUrl;
    }

    public sealed record ManagementSessionDto(
        bool IsAuthenticated,
        string? DisplayName,
        string[] Roles,
        bool IsAdmin,
        bool CanAccessManagementPortal);
}
