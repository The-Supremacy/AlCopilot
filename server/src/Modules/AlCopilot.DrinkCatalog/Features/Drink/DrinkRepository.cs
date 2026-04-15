using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.Drink;

internal sealed class DrinkRepository(DrinkCatalogDbContext dbContext) : IDrinkRepository
{
    public async Task<Drink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await AggregateQuery()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public void Add(Drink aggregate) => dbContext.Drinks.Add(aggregate);

    public void Remove(Drink aggregate) => dbContext.Drinks.Remove(aggregate);

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Drinks.Where(d => d.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(d => d.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Drink?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var local = dbContext.Drinks.Local.FirstOrDefault(d => d.Name == name);
        if (local is not null)
        {
            return local;
        }

        return await AggregateQuery()
            .FirstOrDefaultAsync(d => d.Name == name, cancellationToken);
    }

    private IQueryable<Drink> AggregateQuery()
    {
        return dbContext.Drinks
            .Include(d => d.Tags)
            .Include(d => d.RecipeEntries);
    }
}
