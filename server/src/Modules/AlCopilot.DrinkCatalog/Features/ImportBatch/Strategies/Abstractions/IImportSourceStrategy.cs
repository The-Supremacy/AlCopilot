namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies.Abstractions;

public interface IImportSourceStrategy
{
    ImportStrategyKey Key { get; }

    ValueTask<ImportSourceStrategyResult> CreateImportAsync(
        ImportSourceStrategyRequest request,
        CancellationToken cancellationToken = default);
}
