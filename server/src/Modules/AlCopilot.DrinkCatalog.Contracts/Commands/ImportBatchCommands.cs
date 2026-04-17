using AlCopilot.DrinkCatalog.Contracts.DTOs;
using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Commands;

public sealed record StartImportCommand(
    string StrategyKey,
    string Payload,
    ImportSourceInput Source) : IRequest<ImportBatchDto>;

public sealed record ReviewImportBatchCommand(Guid BatchId) : IRequest<ImportBatchDto>;

public sealed record CancelImportBatchCommand(Guid BatchId) : IRequest<ImportBatchDto>;

public sealed record ApplyImportBatchCommand(Guid BatchId) : IRequest<ImportBatchApplyResultDto>;
