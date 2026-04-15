using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.Tag;

internal sealed class TagQueryService(DrinkCatalogDbContext dbContext) : ITagQueryService
{
    public async Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tags = await dbContext.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.Name)
            .Select(tag => new TagSummaryReadModel(tag.Id, tag.Name))
            .ToListAsync(cancellationToken);

        if (tags.Count == 0)
        {
            return [];
        }

        var tagIds = tags.Select(tag => tag.Id).ToArray();
        var tagCounts = await dbContext.Drinks
            .AsNoTracking()
            .SelectMany(drink => drink.Tags.Select(tag => tag.Id))
            .Where(tagId => tagIds.Contains(tagId))
            .GroupBy(tagId => tagId)
            .Select(group => new { TagId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.TagId, entry => entry.Count, cancellationToken);

        return tags.Select(tag => ToDto(tag, tagCounts)).ToList();
    }

    private sealed record TagSummaryReadModel(Guid Id, string Name);

    private static TagDto ToDto(
        TagSummaryReadModel tag,
        IReadOnlyDictionary<Guid, int> tagCounts)
    {
        return new TagDto(
            tag.Id,
            tag.Name,
            tagCounts.TryGetValue(tag.Id, out var drinkCount) ? drinkCount : 0);
    }
}
