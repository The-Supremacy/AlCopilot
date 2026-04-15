namespace AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;

public interface IImportSourceStrategy
{
    ImportStrategyKey Key { get; }

    ValueTask<ImportSourceStrategyResult> CreateImportAsync(
        ImportSourceStrategyRequest request,
        CancellationToken cancellationToken = default);
}
