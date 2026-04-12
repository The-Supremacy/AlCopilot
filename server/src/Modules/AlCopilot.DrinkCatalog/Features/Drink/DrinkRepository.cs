using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.Drink;

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
        DrinkFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Drinks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var pattern = $"%{filter.SearchQuery.Trim()}%";

            query = query.Where(d =>
                EF.Functions.ILike(d.Name, pattern) ||
                (d.Category != null && EF.Functions.ILike(d.Category!, pattern)) ||
                (d.Description != null && EF.Functions.ILike(d.Description, pattern)) ||
                (d.Method != null && EF.Functions.ILike(d.Method, pattern)) ||
                (d.Garnish != null && EF.Functions.ILike(d.Garnish, pattern)) ||
                d.Tags.Any(t => EF.Functions.ILike(t.Name, pattern)) ||
                d.RecipeEntries.Any(re =>
                    dbContext.Ingredients.Any(i => i.Id == re.IngredientId && EF.Functions.ILike(i.Name, pattern))));
        }

        if (filter.TagIds is { Count: > 0 })
        {
            query = query.Where(d => d.Tags.Any(t => filter.TagIds.Contains(t.Id)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(d => new DrinkDto(
                d.Id,
                d.Name,
                d.Category,
                d.Description,
                d.Method,
                d.Garnish,
                d.ImageUrl,
                d.Tags.Select(t => new TagDto(t.Id, t.Name, dbContext.Drinks.Count(d2 => d2.Tags.Any(dt => dt.Id == t.Id)))).ToList()))
            .ToListAsync(cancellationToken);

        return new PagedResult<DrinkDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<List<DrinkDetailDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Drinks
            .OrderBy(d => d.Name)
            .Select(d => new DrinkDetailDto(
                d.Id,
                d.Name,
                d.Category,
                d.Description,
                d.Method,
                d.Garnish,
                d.ImageUrl,
                d.Tags.Select(t => new TagDto(t.Id, t.Name, dbContext.Drinks.Count(d2 => d2.Tags.Any(dt => dt.Id == t.Id)))).ToList(),
                d.RecipeEntries.Select(re => new RecipeEntryDto(
                    dbContext.Ingredients
                        .Where(i => i.Id == re.IngredientId)
                        .Select(i => new IngredientDto(
                            i.Id,
                            i.Name,
                            i.NotableBrands))
                        .First(),
                    re.Quantity,
                    re.RecommendedBrand)).ToList()))
            .ToListAsync(cancellationToken);
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
                d.Category,
                d.Description,
                d.Method,
                d.Garnish,
                d.ImageUrl,
                d.Tags.Select(t => new TagDto(t.Id, t.Name, dbContext.Drinks.Count(d2 => d2.Tags.Any(dt => dt.Id == t.Id)))).ToList(),
                d.RecipeEntries.Select(re => new RecipeEntryDto(
                    dbContext.Ingredients
                        .Where(i => i.Id == re.IngredientId)
                        .Select(i => new IngredientDto(
                            i.Id,
                            i.Name,
                            i.NotableBrands))
                        .First(),
                    re.Quantity,
                    re.RecommendedBrand)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Drink?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var local = dbContext.Drinks.Local.FirstOrDefault(d => d.Name == name);
        if (local is not null)
            return local;

        return await dbContext.Drinks
            .Include(d => d.Tags)
            .Include(d => d.RecipeEntries)
            .FirstOrDefaultAsync(d => d.Name == name, cancellationToken);
    }
}
