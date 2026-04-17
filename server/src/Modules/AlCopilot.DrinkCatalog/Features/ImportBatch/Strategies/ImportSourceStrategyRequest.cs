namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;

public sealed record ImportSourceStrategyRequest(
    string Payload,
    ImportProvenance Provenance);
