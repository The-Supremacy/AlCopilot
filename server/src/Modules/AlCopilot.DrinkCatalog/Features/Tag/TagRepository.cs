using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.Tag;

internal sealed class TagRepository(DrinkCatalogDbContext dbContext) : ITagRepository
{
    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public void Add(Tag aggregate) => dbContext.Tags.Add(aggregate);

    public void Remove(Tag aggregate) => dbContext.Tags.Remove(aggregate);

    public async Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(
                t.Id,
                t.Name,
                dbContext.Drinks.Count(d => d.Tags.Any(dt => dt.Id == t.Id))))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsReferencedByDrinksAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Drinks.AnyAsync(d => d.Tags.Any(t => t.Id == tagId), cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeTagId = null, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags
            .AnyAsync(t => t.Name == name && (!excludeTagId.HasValue || t.Id != excludeTagId.Value), cancellationToken);
    }

    public async Task<List<Tag>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var local = dbContext.Tags.Local.FirstOrDefault(t => t.Name == name);
        if (local is not null)
            return local;

        return await dbContext.Tags.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }
}
