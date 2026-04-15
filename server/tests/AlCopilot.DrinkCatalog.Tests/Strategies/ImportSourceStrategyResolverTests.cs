using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.Shared.Errors;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Strategies;

public sealed class ImportSourceStrategyResolverTests
{
    [Fact]
    public void GetRequired_ReturnsMatchingStrategy()
    {
        var resolver = new ImportSourceStrategyResolver([new IbaCocktailsSnapshotImportSourceStrategy()]);

        var strategy = resolver.GetRequired("iba-cocktails-snapshot");

        strategy.ShouldBeOfType<IbaCocktailsSnapshotImportSourceStrategy>();
    }

    [Fact]
    public void GetRequired_WhenMissing_Throws()
    {
        var resolver = new ImportSourceStrategyResolver([new IbaCocktailsSnapshotImportSourceStrategy()]);

        Should.Throw<ValidationException>(() => resolver.GetRequired("missing"));
    }
}
