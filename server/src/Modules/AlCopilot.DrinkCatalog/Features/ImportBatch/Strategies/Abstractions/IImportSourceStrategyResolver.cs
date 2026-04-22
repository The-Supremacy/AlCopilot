namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies.Abstractions;

public interface IImportSourceStrategyResolver
{
    IImportSourceStrategy GetRequired(string strategyKey);
    IImportSourceStrategy GetRequired(ImportStrategyKey strategyKey);
}
