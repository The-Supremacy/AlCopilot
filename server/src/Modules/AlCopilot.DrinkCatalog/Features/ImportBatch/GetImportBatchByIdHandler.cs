using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed class GetImportBatchByIdHandler(IImportBatchRepository importBatchRepository)
    : IRequestHandler<GetImportBatchByIdQuery, ImportBatchDto?>
{
    public async ValueTask<ImportBatchDto?> Handle(GetImportBatchByIdQuery request, CancellationToken cancellationToken)
    {
        var batch = await importBatchRepository.GetByIdAsync(request.BatchId, cancellationToken);
        return batch?.ToDto();
    }
}
