using System.Security.Claims;
using AlCopilot.Shared.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

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
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActorAccessor, HttpContextCurrentActorAccessor>();

        services.AddAuthentication(authentication =>
            {
                authentication.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                authentication.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(cookie =>
            {
                cookie.Cookie.Name = options.CookieName;
                cookie.Cookie.HttpOnly = true;
                cookie.Cookie.SameSite = SameSiteMode.Lax;
                cookie.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
            })
            .AddOpenIdConnect(openIdConnect =>
            {
                openIdConnect.Authority = options.Authority;
                openIdConnect.ClientId = options.ClientId;
                openIdConnect.ClientSecret = options.ClientSecret;
                openIdConnect.ResponseType = "code";
                openIdConnect.UsePkce = true;
                openIdConnect.SaveTokens = false;
                openIdConnect.GetClaimsFromUserInfoEndpoint = false;
                openIdConnect.RequireHttpsMetadata = !environment.IsDevelopment();
                openIdConnect.CallbackPath = options.CallbackPath;
                openIdConnect.SignedOutCallbackPath = options.SignedOutCallbackPath;
                openIdConnect.MapInboundClaims = false;
                openIdConnect.Scope.Clear();
                openIdConnect.Scope.Add("openid");
                openIdConnect.Scope.Add("profile");
                openIdConnect.Scope.Add("email");
                openIdConnect.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles",
                };
            });

        return services;
    }

    public static IServiceCollection AddManagementAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                ManagementAuthorizationPolicies.CanAccessManagementPortal,
                policy => policy.RequireRole("manager", "admin"));

            options.AddPolicy(
                ManagementAuthorizationPolicies.CanAdministerManagement,
                policy => policy.RequireRole("admin"));
        });

        return services;
    }
}
