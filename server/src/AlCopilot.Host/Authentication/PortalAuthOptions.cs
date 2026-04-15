namespace AlCopilot.Host.Authentication;

public abstract class PortalAuthOptions
{
    public string Authority { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string CookieName { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = string.Empty;

    public string SignedOutCallbackPath { get; set; } = string.Empty;
}
