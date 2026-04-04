using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

internal sealed class IngredientRepository(DrinkCatalogDbContext dbContext) : IIngredientRepository
{
    public async Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Ingredients.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public void Add(Ingredient aggregate) => dbContext.Ingredients.Add(aggregate);

    public void Remove(Ingredient aggregate) => dbContext.Ingredients.Remove(aggregate);

    public async Task<List<IngredientDto>> GetAllAsync(Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Ingredients.AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(i => i.IngredientCategoryId == categoryId.Value);

        return await query
            .OrderBy(i => i.Name)
            .Select(i => new IngredientDto(
                i.Id,
                i.Name,
                dbContext.IngredientCategories
                    .Where(c => c.Id == i.IngredientCategoryId)
                    .Select(c => new IngredientCategoryDto(c.Id, c.Name))
                    .First(),
                i.NotableBrands))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await dbContext.Ingredients.AnyAsync(i => i.Name == name, cancellationToken);
    }
}
