namespace AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;

public sealed record ImportSourceStrategyResult(
    ImportProvenance Provenance,
    NormalizedCatalogImport Import,
    List<ImportDiagnostic> Diagnostics);
