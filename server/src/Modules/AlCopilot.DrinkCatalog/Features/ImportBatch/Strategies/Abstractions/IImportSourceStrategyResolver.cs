namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;

public interface IImportSourceStrategyResolver
{
    IImportSourceStrategy GetRequired(string strategyKey);
    IImportSourceStrategy GetRequired(ImportStrategyKey strategyKey);
}
