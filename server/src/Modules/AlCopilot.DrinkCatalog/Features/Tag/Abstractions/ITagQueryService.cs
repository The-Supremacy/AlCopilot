using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.Tag.Abstractions;

public interface ITagQueryService
{
    Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
