namespace AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;

public interface IImportSourceStrategyResolver
{
    IImportSourceStrategy GetRequired(string strategyKey);
    IImportSourceStrategy GetRequired(ImportStrategyKey strategyKey);
}
