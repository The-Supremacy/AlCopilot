namespace AlCopilot.Host.Authentication;

public static class ManagementAuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddManagementAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = configuration.GetSection(ManagementAuthOptions.SectionName).Get<ManagementAuthOptions>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{ManagementAuthOptions.SectionName}' is required.");

        if (string.IsNullOrWhiteSpace(options.Authority) ||
            string.IsNullOrWhiteSpace(options.ClientId) ||
            string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            throw new InvalidOperationException(
                $"Configuration section '{ManagementAuthOptions.SectionName}' must define Authority, ClientId, and ClientSecret.");
        }

        services.AddSingleton(options);
        services.AddPortalAuthenticationCore();
        services.AddAuthentication()
            .AddPortalCookie(PortalAuthenticationSchemes.ManagementCookieScheme, options.CookieName, environment)
            .AddPortalOpenIdConnect(PortalAuthenticationSchemes.ManagementOidcScheme, options, environment);

        return services;
    }
}
