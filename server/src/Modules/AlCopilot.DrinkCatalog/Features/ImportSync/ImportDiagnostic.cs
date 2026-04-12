namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportDiagnostic(
    int? RowNumber,
    string Code,
    string Message,
    string Severity);
