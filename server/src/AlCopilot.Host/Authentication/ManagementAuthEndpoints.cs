using System.Security.Claims;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authentication;

namespace AlCopilot.Host.Authentication;

public static class ManagementAuthEndpoints
{
    public static IEndpointRouteBuilder MapManagementAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth/management");

        group.MapGet("/session", (ClaimsPrincipal user) => Results.Ok(CreateSession(user)))
            .AllowAnonymous();

        group.MapGet("/login", (string? returnUrl, string? prompt) =>
            Results.Challenge(
                CreateLoginProperties(returnUrl, prompt),
                [PortalAuthenticationSchemes.ManagementOidcScheme]))
            .AllowAnonymous();

        group.MapGet("/account-management", (ManagementAuthOptions options) =>
                Results.Redirect(BuildAccountManagementUrl(options.Authority)))
            .AllowAnonymous();

        group.MapPost("/logout", (string? returnUrl) =>
            Results.SignOut(
                CreateLogoutProperties(returnUrl),
                [
                    PortalAuthenticationSchemes.ManagementCookieScheme,
                    PortalAuthenticationSchemes.ManagementOidcScheme,
                ]));

        group.MapPost("/switch-account", (string? returnUrl) =>
            Results.SignOut(
                CreateSwitchAccountProperties(returnUrl),
                [
                    PortalAuthenticationSchemes.ManagementCookieScheme,
                    PortalAuthenticationSchemes.ManagementOidcScheme,
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

    private static string BuildAccountManagementUrl(string authority)
    {
        var normalizedAuthority = authority.TrimEnd('/');
        return $"{normalizedAuthority}/account";
    }

    private static string BuildSwitchAccountRedirectUrl(string? returnUrl)
    {
        var loginUrl = "/api/auth/management/login";
        return QueryHelpers.AddQueryString(loginUrl, new Dictionary<string, string?>
        {
            ["returnUrl"] = SanitizeReturnUrl(returnUrl),
            ["prompt"] = "login",
        });
    }

    public sealed record ManagementSessionDto(
        bool IsAuthenticated,
        string? DisplayName,
        string[] Roles,
        bool IsAdmin,
        bool CanAccessManagementPortal);
}
