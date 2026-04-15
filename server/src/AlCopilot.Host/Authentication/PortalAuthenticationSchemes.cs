namespace AlCopilot.Host.Authentication;

public static class PortalAuthenticationSchemes
{
    public const string PolicyScheme = "PortalPolicy";
    public const string ManagementCookieScheme = "ManagementCookies";
    public const string ManagementOidcScheme = "ManagementOpenIdConnect";
    public const string CustomerCookieScheme = "CustomerCookies";
    public const string CustomerOidcScheme = "CustomerOpenIdConnect";
}
