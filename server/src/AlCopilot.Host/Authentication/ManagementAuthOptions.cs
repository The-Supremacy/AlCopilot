namespace AlCopilot.Host.Authentication;

public sealed class ManagementAuthOptions : PortalAuthOptions
{
    public const string SectionName = "Authentication:Management";

    public ManagementAuthOptions()
    {
        CookieName = ".AlCopilot.Management.Auth";
        CallbackPath = "/api/auth/management/signin-oidc";
        SignedOutCallbackPath = "/api/auth/management/signout-callback-oidc";
    }
}
