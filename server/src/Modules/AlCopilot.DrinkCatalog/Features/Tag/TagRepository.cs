using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Data.Repositories;

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
            .Select(t => new TagDto(t.Id, t.Name, t.Drinks.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsReferencedByDrinksAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Drinks.AnyAsync(d => d.Tags.Any(t => t.Id == tagId), cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags.AnyAsync(t => t.Name == name, cancellationToken);
    }
}
