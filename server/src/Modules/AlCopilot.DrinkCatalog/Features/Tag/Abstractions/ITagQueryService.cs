using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.Tag;

public interface ITagQueryService
{
    Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
