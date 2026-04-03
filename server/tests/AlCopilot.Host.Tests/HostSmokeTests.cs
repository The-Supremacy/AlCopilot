using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace AlCopilot.Host.Tests;

public sealed class HostSmokeTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
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
}
