using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
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

    public async Task<List<IngredientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Ingredients
            .OrderBy(i => i.Name)
            .Select(i => new IngredientDto(
                i.Id,
                i.Name,
                i.NotableBrands))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludingIngredientId = null, CancellationToken cancellationToken = default)
    {
        return await dbContext.Ingredients.AnyAsync(
            i => i.Name == name && (!excludingIngredientId.HasValue || i.Id != excludingIngredientId.Value),
            cancellationToken);
    }

    public async Task<Ingredient?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var local = dbContext.Ingredients.Local.FirstOrDefault(i => i.Name == name);
        if (local is not null)
            return local;

        return await dbContext.Ingredients.FirstOrDefaultAsync(i => i.Name == name, cancellationToken);
    }

    public async Task<bool> IsReferencedByActiveDrinksAsync(Guid ingredientId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Drinks.AnyAsync(
            d => d.RecipeEntries.Any(re => re.IngredientId == ingredientId),
            cancellationToken);
    }
}
