namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed record ImportDiagnostic(
    int? RowNumber,
    string Code,
    string Message,
    string Severity);
