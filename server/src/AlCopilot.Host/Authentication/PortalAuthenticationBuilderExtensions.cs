using AlCopilot.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace AlCopilot.Host.Authentication;

internal static class PortalAuthenticationBuilderExtensions
{
    public static IServiceCollection AddPortalAuthenticationCore(this IServiceCollection services)
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(PortalAuthenticationCoreMarker)))
        {
            return services;
        }

        services.TryAddSingleton<PortalAuthenticationCoreMarker>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActorAccessor, HttpContextCurrentActorAccessor>();

        services.AddAuthentication(authentication =>
            {
                authentication.DefaultScheme = PortalAuthenticationSchemes.PolicyScheme;
                authentication.DefaultChallengeScheme = PortalAuthenticationSchemes.PolicyScheme;
                authentication.DefaultSignInScheme = PortalAuthenticationSchemes.PolicyScheme;
            })
            .AddPolicyScheme(
                PortalAuthenticationSchemes.PolicyScheme,
                PortalAuthenticationSchemes.PolicyScheme,
                policy =>
                {
                    policy.ForwardDefaultSelector = context =>
                    {
                        var path = context.Request.Path;

                        if (path.StartsWithSegments("/api/auth/customer", StringComparison.Ordinal) ||
                            path.StartsWithSegments("/api/customer", StringComparison.Ordinal))
                        {
                            return PortalAuthenticationSchemes.CustomerCookieScheme;
                        }

                        return PortalAuthenticationSchemes.ManagementCookieScheme;
                    };
                });

        return services;
    }

    public static IServiceCollection AddPortalAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                ManagementAuthorizationPolicies.CanAccessManagementPortal,
                policy => policy.RequireRole("manager", "admin"));

            options.AddPolicy(
                ManagementAuthorizationPolicies.CanAdministerManagement,
                policy => policy.RequireRole("admin"));

            options.AddPolicy(
                CustomerAuthorizationPolicies.CanAccessCustomerPortal,
                policy => policy.RequireRole("user"));
        });

        return services;
    }

    public static AuthenticationBuilder AddPortalCookie(
        this AuthenticationBuilder authenticationBuilder,
        string scheme,
        string cookieName,
        IHostEnvironment environment)
    {
        authenticationBuilder.AddCookie(
            scheme,
            cookie =>
            {
                cookie.Cookie.Name = cookieName;
                cookie.Cookie.HttpOnly = true;
                cookie.Cookie.SameSite = SameSiteMode.Lax;
                cookie.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                cookie.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.Ordinal))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.Ordinal))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    },
                };
            });

        return authenticationBuilder;
    }

    public static AuthenticationBuilder AddPortalOpenIdConnect(
        this AuthenticationBuilder authenticationBuilder,
        string scheme,
        PortalAuthOptions options,
        IHostEnvironment environment)
    {
        authenticationBuilder.AddOpenIdConnect(
            scheme,
            openIdConnect =>
            {
                openIdConnect.Authority = options.Authority;
                openIdConnect.ClientId = options.ClientId;
                openIdConnect.ClientSecret = options.ClientSecret;
                openIdConnect.ResponseType = "code";
                openIdConnect.UsePkce = true;
                openIdConnect.SaveTokens = true;
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

        return authenticationBuilder;
    }
}

internal sealed class PortalAuthenticationCoreMarker;
