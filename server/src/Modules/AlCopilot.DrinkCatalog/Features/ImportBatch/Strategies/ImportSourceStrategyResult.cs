namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;

public sealed record ImportSourceStrategyResult(
    ImportProvenance Provenance,
    NormalizedCatalogImport Import,
    List<ImportDiagnostic> Diagnostics);
