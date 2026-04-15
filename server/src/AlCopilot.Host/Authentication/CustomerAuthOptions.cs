namespace AlCopilot.Host.Authentication;

public sealed class CustomerAuthOptions : PortalAuthOptions
{
    public const string SectionName = "Authentication:Customer";

    public CustomerAuthOptions()
    {
        CookieName = ".AlCopilot.Customer.Auth";
        CallbackPath = "/api/auth/customer/signin-oidc";
        SignedOutCallbackPath = "/api/auth/customer/signout-callback-oidc";
    }

    public string RegisterPath { get; init; } = "/api/auth/customer/register";
}
