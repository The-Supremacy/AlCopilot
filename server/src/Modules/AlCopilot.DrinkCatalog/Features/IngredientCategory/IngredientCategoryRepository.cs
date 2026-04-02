using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.IngredientCategory;

internal sealed class IngredientCategoryRepository(DrinkCatalogDbContext dbContext) : IIngredientCategoryRepository
{
    public async Task<IngredientCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.IngredientCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void Add(IngredientCategory aggregate) => dbContext.IngredientCategories.Add(aggregate);

    public void Remove(IngredientCategory aggregate) => dbContext.IngredientCategories.Remove(aggregate);

    public async Task<List<IngredientCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.IngredientCategories
            .OrderBy(c => c.Name)
            .Select(c => new IngredientCategoryDto(c.Id, c.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await dbContext.IngredientCategories.AnyAsync(c => c.Name == name, cancellationToken);
    }
}
