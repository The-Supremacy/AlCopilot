namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public enum ImportBatchApplyReadiness
{
    Ready = 0,
    RequiresReview = 1,
    BlockedByValidationErrors = 2,
    Completed = 3,
    Cancelled = 4,
}
