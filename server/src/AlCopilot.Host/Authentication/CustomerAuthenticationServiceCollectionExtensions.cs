namespace AlCopilot.Host.Authentication;

public static class CustomerAuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = configuration.GetSection(CustomerAuthOptions.SectionName).Get<CustomerAuthOptions>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{CustomerAuthOptions.SectionName}' is required.");

        if (string.IsNullOrWhiteSpace(options.Authority) ||
            string.IsNullOrWhiteSpace(options.ClientId) ||
            string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            throw new InvalidOperationException(
                $"Configuration section '{CustomerAuthOptions.SectionName}' must define Authority, ClientId, and ClientSecret.");
        }

        services.AddSingleton(options);
        services.AddPortalAuthenticationCore();
        services.AddAuthentication()
            .AddPortalCookie(PortalAuthenticationSchemes.CustomerCookieScheme, options.CookieName, environment)
            .AddPortalOpenIdConnect(PortalAuthenticationSchemes.CustomerOidcScheme, options, environment);

        return services;
    }
}
