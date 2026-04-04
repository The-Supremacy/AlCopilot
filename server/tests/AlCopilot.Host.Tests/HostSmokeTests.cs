using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace AlCopilot.Host.Tests;

public sealed class HostSmokeTests(HostSmokeTests.SmokeTestFactory factory)
    : IClassFixture<HostSmokeTests.SmokeTestFactory>
{
    [Fact]
    public async Task Root_ReturnsSuccessStatusCode()
    {
        const string connectionStringKey = "ConnectionStrings__drink-catalog";
        var originalValue = Environment.GetEnvironmentVariable(connectionStringKey);

        Environment.SetEnvironmentVariable(
            connectionStringKey,
            "Host=localhost;Database=alcopilot;Username=alcopilot;Password=alcopilot");

        try
        {
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable(connectionStringKey, originalValue);
        }
    }

    public sealed class SmokeTestFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:drink-catalog", "Host=localhost;Database=test;Username=test;Password=test");
        }
    }
}
