using AlCopilot.Shared.Errors;

namespace AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;

internal sealed class ImportSourceStrategyResolver(
    IEnumerable<IImportSourceStrategy> strategies) : IImportSourceStrategyResolver
{
    private readonly Dictionary<ImportStrategyKey, IImportSourceStrategy> _strategies = strategies
        .ToDictionary(strategy => strategy.Key);

    public IImportSourceStrategy GetRequired(string strategyKey)
    {
        return GetRequired(ImportStrategyKeyExtensions.Parse(strategyKey));
    }

    public IImportSourceStrategy GetRequired(ImportStrategyKey strategyKey)
    {
        if (_strategies.TryGetValue(strategyKey, out var strategy))
            return strategy;

        throw new ValidationException($"Import strategy '{strategyKey.ToWireValue()}' is not registered.");
    }
}
