namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;

public interface IImportSourceStrategy
{
    ImportStrategyKey Key { get; }

    ValueTask<ImportSourceStrategyResult> CreateImportAsync(
        ImportSourceStrategyRequest request,
        CancellationToken cancellationToken = default);
}
