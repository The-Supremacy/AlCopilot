using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Data.Repositories;

internal sealed class DrinkRepository(DrinkCatalogDbContext dbContext) : IDrinkRepository
{
    public async Task<Drink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Drinks
            .Include(d => d.Tags)
            .Include(d => d.RecipeEntries)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public void Add(Drink aggregate) => dbContext.Drinks.Add(aggregate);

    public void Remove(Drink aggregate) => dbContext.Drinks.Remove(aggregate);

    public async Task<PagedResult<DrinkDto>> GetPagedAsync(
        List<Guid>? tagIds, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Drinks.AsQueryable();

        if (tagIds is { Count: > 0 })
        {
            query = query.Where(d => d.Tags.Any(t => tagIds.Contains(t.Id)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DrinkDto(
                d.Id,
                d.Name,
                d.Description,
                d.ImageUrl,
                d.Tags.Select(t => new TagDto(t.Id, t.Name, t.Drinks.Count)).ToList()))
            .ToListAsync(cancellationToken);

        return new PagedResult<DrinkDto>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<DrinkDto>> SearchAsync(
        string searchQuery, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var lowerQuery = searchQuery.ToLowerInvariant();

        var query = dbContext.Drinks
            .Where(d =>
                EF.Functions.ILike(d.Name, $"%{lowerQuery}%") ||
                (d.Description != null && EF.Functions.ILike(d.Description, $"%{lowerQuery}%")) ||
                d.Tags.Any(t => EF.Functions.ILike(t.Name, $"%{lowerQuery}%")) ||
                d.RecipeEntries.Any(re =>
                    dbContext.Ingredients.Any(i => i.Id == re.IngredientId && EF.Functions.ILike(i.Name, $"%{lowerQuery}%"))));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DrinkDto(
                d.Id,
                d.Name,
                d.Description,
                d.ImageUrl,
                d.Tags.Select(t => new TagDto(t.Id, t.Name, t.Drinks.Count)).ToList()))
            .ToListAsync(cancellationToken);

        return new PagedResult<DrinkDto>(items, totalCount, page, pageSize);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Drinks.Where(d => d.Name == name);
        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<DrinkDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Drinks
            .Where(d => d.Id == id)
            .Select(d => new DrinkDetailDto(
                d.Id,
                d.Name,
                d.Description,
                d.ImageUrl,
                d.Tags.Select(t => new TagDto(t.Id, t.Name, t.Drinks.Count)).ToList(),
                d.RecipeEntries.Select(re => new RecipeEntryDto(
                    dbContext.Ingredients
                        .Where(i => i.Id == re.IngredientId)
                        .Select(i => new IngredientDto(
                            i.Id,
                            i.Name,
                            dbContext.IngredientCategories
                                .Where(c => c.Id == i.IngredientCategoryId)
                                .Select(c => new IngredientCategoryDto(c.Id, c.Name))
                                .First(),
                            i.NotableBrands))
                        .First(),
                    re.Quantity,
                    re.RecommendedBrand)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
