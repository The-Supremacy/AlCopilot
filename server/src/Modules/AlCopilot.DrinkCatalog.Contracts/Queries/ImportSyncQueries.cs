using AlCopilot.DrinkCatalog.Contracts.DTOs;
using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Queries;

public sealed record GetImportBatchByIdQuery(Guid BatchId) : IRequest<ImportBatchDto?>;

public sealed record GetImportHistoryQuery : IRequest<List<ImportBatchDto>>;

public sealed record GetAuditLogEntriesQuery : IRequest<List<AuditLogEntryDto>>;
