using AlCopilot.Shared.Errors;

namespace AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;

public enum ImportStrategyKey
{
    IbaCocktailsSnapshot = 1,
}

internal static class ImportStrategyKeyExtensions
{
    public static string ToWireValue(this ImportStrategyKey strategyKey) =>
        strategyKey switch
        {
            ImportStrategyKey.IbaCocktailsSnapshot => "iba-cocktails-snapshot",
            _ => throw new ArgumentOutOfRangeException(nameof(strategyKey), strategyKey, "Unknown import strategy."),
        };

    public static ImportStrategyKey Parse(string strategyKey)
    {
        return strategyKey.Trim().ToLowerInvariant() switch
        {
            "iba-cocktails-snapshot" => ImportStrategyKey.IbaCocktailsSnapshot,
            _ => throw new ValidationException($"Import strategy '{strategyKey}' is not registered."),
        };
    }
}
