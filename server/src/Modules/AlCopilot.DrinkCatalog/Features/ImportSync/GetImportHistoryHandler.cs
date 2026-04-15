using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed class GetImportHistoryHandler(IImportBatchRepository importBatchRepository)
    : IRequestHandler<GetImportHistoryQuery, List<ImportBatchDto>>
{
    public async ValueTask<List<ImportBatchDto>> Handle(GetImportHistoryQuery request, CancellationToken cancellationToken)
    {
        var batches = await importBatchRepository.GetHistoryAsync(cancellationToken);
        return batches.Select(b => b.ToDto()).ToList();
    }
}
