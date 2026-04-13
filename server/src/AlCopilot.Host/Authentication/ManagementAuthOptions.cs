namespace AlCopilot.Host.Authentication;

public sealed class ManagementAuthOptions
{
    public const string SectionName = "Authentication:Management";

    public string Authority { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string CookieName { get; init; } = ".AlCopilot.Management.Auth";

    public string CallbackPath { get; init; } = "/api/auth/management/signin-oidc";

    public string SignedOutCallbackPath { get; init; } = "/api/auth/management/signout-callback-oidc";
}
