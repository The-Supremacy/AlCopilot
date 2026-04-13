using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AlCopilot.Testing.Shared;
using Shouldly;

namespace AlCopilot.Host.Tests;

public sealed class ManagementAuthenticationTests : IClassFixture<ManagementAuthenticationTests.ManagementAuthWebApplicationFactory>
{
    private readonly ManagementAuthWebApplicationFactory _factory;

    public ManagementAuthenticationTests(ManagementAuthWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SessionEndpoint_ReturnsAnonymousState_WhenUserIsNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/management/session");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<ManagementSessionResponse>();
        session.ShouldNotBeNull();
        session!.IsAuthenticated.ShouldBeFalse();
        session.CanAccessManagementPortal.ShouldBeFalse();
    }

    [Fact]
    public async Task ManagementEndpoint_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/drink-catalog/tags/");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ManagementEndpoint_ReturnsForbidden_WhenUserLacksManagementRole()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeaderName, "user@alcopilot.local");
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeaderName, "user");

        var response = await client.GetAsync("/api/drink-catalog/tags/");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SessionEndpoint_ReturnsManagementCapabilities_WhenUserHasManagerRole()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeaderName, "manager@alcopilot.local");
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeaderName, "manager,user");

        var response = await client.GetAsync("/api/auth/management/session");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var session = await response.Content.ReadFromJsonAsync<ManagementSessionResponse>();
        session.ShouldNotBeNull();
        session!.IsAuthenticated.ShouldBeTrue();
        session.DisplayName.ShouldBe("manager@alcopilot.local");
        session.CanAccessManagementPortal.ShouldBeTrue();
    }

    public sealed class ManagementAuthWebApplicationFactory : BackendIntegrationWebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:drink-catalog", "Host=localhost;Database=alcopilot-tests;Username=postgres;Password=postgres");
            base.ConfigureWebHost(builder);
        }

        protected override IReadOnlyDictionary<string, string?> CreateConfigurationOverrides() =>
            new Dictionary<string, string?>
            {
                ["Authentication:Management:Authority"] = "http://localhost:8080/realms/alcopilot",
                ["Authentication:Management:ClientId"] = "alcopilot-management-portal",
                ["Authentication:Management:ClientSecret"] = "alcopilot-management-dev-secret",
            };

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthHandler.SchemeName;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        }
    }

    public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Test";
        public const string UserHeaderName = "X-Test-User";
        public const string RolesHeaderName = "X-Test-Roles";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserHeaderName, out var username) ||
                string.IsNullOrWhiteSpace(username))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username.ToString()),
                new("preferred_username", username.ToString()),
            };

            if (Request.Headers.TryGetValue(RolesHeaderName, out var roles))
            {
                foreach (var role in roles.ToString()
                             .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    claims.Add(new Claim("roles", role));
                }
            }

            var identity = new ClaimsIdentity(claims, SchemeName, ClaimTypes.Name, "roles");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public sealed record ManagementSessionResponse(
        bool IsAuthenticated,
        string? DisplayName,
        string[] Roles,
        bool IsAdmin,
        bool CanAccessManagementPortal);
}
