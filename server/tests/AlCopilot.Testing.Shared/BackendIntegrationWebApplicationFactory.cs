using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.Testing.Shared;

public abstract class BackendIntegrationWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected virtual string EnvironmentName => "Testing";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var overrides = CreateConfigurationOverrides();
            if (overrides.Count > 0)
            {
                configurationBuilder.AddInMemoryCollection(overrides);
            }

            ConfigureAdditionalConfiguration(configurationBuilder);
        });
        builder.ConfigureTestServices(ConfigureTestServices);
    }

    protected virtual IReadOnlyDictionary<string, string?> CreateConfigurationOverrides() =>
        new Dictionary<string, string?>();

    protected virtual void ConfigureAdditionalConfiguration(IConfigurationBuilder configurationBuilder)
    {
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
    }
}
