namespace AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;

public sealed record ImportSourceStrategyRequest(
    string Payload,
    ImportProvenance Provenance);
