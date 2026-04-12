namespace AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;

public sealed record ImportSourceStrategyResult(
    string SourceFingerprint,
    ImportProvenance Provenance,
    NormalizedCatalogImport Import,
    List<ImportDiagnostic> Diagnostics);
