using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;

namespace AlCopilot.Host.Authentication;

public static class CustomerAuthEndpoints
{
    public static IEndpointRouteBuilder MapCustomerAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth/customer");

        group.MapGet("/session", (ClaimsPrincipal user) => Results.Ok(CreateSession(user)))
            .AllowAnonymous();

        group.MapGet("/login", (string? returnUrl, string? prompt) =>
            Results.Challenge(
                CreateLoginProperties(returnUrl, prompt),
                [PortalAuthenticationSchemes.CustomerOidcScheme]))
            .AllowAnonymous();

        group.MapGet("/register", (HttpContext context, string? returnUrl) =>
            Results.Challenge(
                CreateRegisterProperties(returnUrl),
                [PortalAuthenticationSchemes.CustomerOidcScheme]))
            .AllowAnonymous();

        group.MapPost("/logout", (string? returnUrl) =>
            Results.SignOut(
                CreateLogoutProperties(returnUrl),
                [
                    PortalAuthenticationSchemes.CustomerCookieScheme,
                    PortalAuthenticationSchemes.CustomerOidcScheme,
                ]));

        group.MapPost("/switch-account", (string? returnUrl) =>
            Results.SignOut(
                CreateSwitchAccountProperties(returnUrl),
                [
                    PortalAuthenticationSchemes.CustomerCookieScheme,
                    PortalAuthenticationSchemes.CustomerOidcScheme,
                ]));

        return endpoints;
    }

    private static AuthenticationProperties CreateLoginProperties(string? returnUrl, string? prompt)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = SanitizeReturnUrl(returnUrl),
        };

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            properties.Parameters["prompt"] = prompt.Trim();
        }

        return properties;
    }

    private static AuthenticationProperties CreateRegisterProperties(string? returnUrl)
    {
        var properties = CreateLoginProperties(returnUrl, null);
        properties.Parameters["screen_hint"] = "signup";
        return properties;
    }

    private static AuthenticationProperties CreateLogoutProperties(string? returnUrl) =>
        new()
        {
            RedirectUri = SanitizeReturnUrl(returnUrl),
        };

    private static AuthenticationProperties CreateSwitchAccountProperties(string? returnUrl) =>
        new()
        {
            RedirectUri = BuildSwitchAccountRedirectUrl(returnUrl),
        };

    private static CustomerSessionDto CreateSession(ClaimsPrincipal user)
    {
        var roles = user.Claims
            .Where(claim => string.Equals(claim.Type, "roles", StringComparison.Ordinal) ||
                            string.Equals(claim.Type, ClaimTypes.Role, StringComparison.Ordinal))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();

        var isAuthenticated = user.Identity?.IsAuthenticated is true;
        return new CustomerSessionDto(
            isAuthenticated,
            isAuthenticated ? user.FindFirstValue("preferred_username") ?? user.Identity?.Name : null,
            roles,
            roles.Contains("user", StringComparer.Ordinal));
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

    private static string BuildSwitchAccountRedirectUrl(string? returnUrl)
    {
        var loginUrl = "/api/auth/customer/login";
        return QueryHelpers.AddQueryString(loginUrl, new Dictionary<string, string?>
        {
            ["returnUrl"] = SanitizeReturnUrl(returnUrl),
            ["prompt"] = "login",
        });
    }

    public sealed record CustomerSessionDto(
        bool IsAuthenticated,
        string? DisplayName,
        string[] Roles,
        bool CanAccessCustomerPortal);
}
