using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Tag;

public sealed class GetTagsHandler(ITagRepository tagRepository)
    : IRequestHandler<GetTagsQuery, List<TagDto>>
{
    public async ValueTask<List<TagDto>> Handle(
        GetTagsQuery request, CancellationToken cancellationToken)
    {
        return await tagRepository.GetAllAsync(cancellationToken);
    }
}
