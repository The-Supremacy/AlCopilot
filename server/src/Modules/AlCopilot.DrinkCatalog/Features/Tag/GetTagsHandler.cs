using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Queries;

public sealed class GetTagsHandler(ITagRepository tagRepository)
    : IRequestHandler<GetTagsQuery, List<TagDto>>
{
    public async ValueTask<List<TagDto>> Handle(
        GetTagsQuery request, CancellationToken cancellationToken)
    {
        return await tagRepository.GetAllAsync(cancellationToken);
    }
}
