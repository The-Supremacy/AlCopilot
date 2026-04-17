using AlCopilot.DrinkCatalog;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Services;

public sealed class DrinkCatalogModuleTests
{
    [Fact]
    public void AddDrinkCatalogModule_RegistersSplitImportServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:drink-catalog"] = "Host=localhost;Database=test;Username=test;Password=test"
                })
            .Build();

        services.AddDrinkCatalogModule(configuration);

        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IImportBatchProcessingService)
            && sd.ImplementationType == typeof(ImportBatchProcessingService));
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(IImportBatchApplyService)
            && sd.ImplementationType == typeof(ImportBatchApplyService));
    }
}
