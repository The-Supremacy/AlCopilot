using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.Shared.Models;
using AlCopilot.Shared.Text;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.Drink;

internal sealed class DrinkQueryService(DrinkCatalogDbContext dbContext) : IDrinkQueryService
{
    public async Task<PagedResult<DrinkDto>> GetPagedAsync(
        DrinkFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Drinks.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var pattern = $"%{filter.SearchQuery.TrimOrEmpty()}%";

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

        var drinks = await query
            .OrderBy(d => d.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(d => new DrinkListReadModel(
                d.Id,
                d.Name,
                d.Category,
                d.Description,
                d.Method,
                d.Garnish,
                d.ImageUrl,
                d.Tags.Select(t => new TagReadModel(t.Id, t.Name)).ToList()))
            .ToListAsync(cancellationToken);

        var tagCounts = await LoadTagCountsAsync(
            drinks.SelectMany(drink => drink.Tags).Select(tag => tag.Id),
            cancellationToken);

        return new PagedResult<DrinkDto>(
            drinks.Select(drink => drink.ToDto(tagCounts)).ToList(),
            totalCount,
            filter.Page,
            filter.PageSize);
    }

    public async Task<List<DrinkDetailDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var drinks = await dbContext.Drinks
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DrinkDetailReadModel(
                d.Id,
                d.Name,
                d.Category,
                d.Description,
                d.Method,
                d.Garnish,
                d.ImageUrl,
                d.Tags.Select(t => new TagReadModel(t.Id, t.Name)).ToList(),
                d.RecipeEntries.Select(re => new RecipeEntryReadModel(
                    re.IngredientId,
                    re.Quantity,
                    re.RecommendedBrand)).ToList()))
            .ToListAsync(cancellationToken);

        return await MapDrinkDetailsAsync(drinks, cancellationToken);
    }

    public async Task<DrinkDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var drink = await dbContext.Drinks
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DrinkDetailReadModel(
                d.Id,
                d.Name,
                d.Category,
                d.Description,
                d.Method,
                d.Garnish,
                d.ImageUrl,
                d.Tags.Select(t => new TagReadModel(t.Id, t.Name)).ToList(),
                d.RecipeEntries.Select(re => new RecipeEntryReadModel(
                    re.IngredientId,
                    re.Quantity,
                    re.RecommendedBrand)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (drink is null)
        {
            return null;
        }

        return (await MapDrinkDetailsAsync([drink], cancellationToken)).Single();
    }

    public async Task<List<FuzzyDrinkMatchDto>> FindFuzzyDrinkMatchesAsync(
        string searchText,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var normalized = searchText.NullIfWhiteSpace();
        if (normalized is null)
        {
            return [];
        }

        var clampedLimit = Math.Clamp(limit, 1, 10);

        return await dbContext.Drinks
            .AsNoTracking()
            .Select(drink => new
            {
                drink.Id,
                Name = (string)drink.Name,
                Similarity = EF.Functions.TrigramsSimilarity(drink.Name, normalized)
            })
            .Where(match => match.Similarity >= 0.30d)
            .OrderByDescending(match => match.Similarity)
            .ThenBy(match => match.Name)
            .Take(clampedLimit)
            .Select(match => new FuzzyDrinkMatchDto(
                match.Id,
                match.Name,
                match.Similarity))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FuzzyIngredientMatchDto>> FindFuzzyIngredientMatchesAsync(
        string searchText,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var normalized = searchText.NullIfWhiteSpace();
        if (normalized is null)
        {
            return [];
        }

        var clampedLimit = Math.Clamp(limit, 1, 10);

        return await dbContext.Ingredients
            .AsNoTracking()
            .Select(ingredient => new
            {
                ingredient.Id,
                Name = (string)ingredient.Name,
                Similarity = EF.Functions.TrigramsSimilarity(ingredient.Name, normalized)
            })
            .Where(match => match.Similarity >= 0.30d)
            .OrderByDescending(match => match.Similarity)
            .ThenBy(match => match.Name)
            .Take(clampedLimit)
            .Select(match => new FuzzyIngredientMatchDto(
                match.Id,
                match.Name,
                match.Similarity))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<DrinkDetailDto>> MapDrinkDetailsAsync(
        IReadOnlyCollection<DrinkDetailReadModel> drinks,
        CancellationToken cancellationToken)
    {
        if (drinks.Count == 0)
        {
            return [];
        }

        var tagCounts = await LoadTagCountsAsync(
            drinks.SelectMany(drink => drink.Tags).Select(tag => tag.Id),
            cancellationToken);
        var ingredients = await LoadIngredientsAsync(
            drinks.SelectMany(drink => drink.RecipeEntries).Select(entry => entry.IngredientId),
            cancellationToken);

        return drinks.Select(drink => drink.ToDto(tagCounts, ingredients)).ToList();
    }

    private async Task<Dictionary<Guid, int>> LoadTagCountsAsync(
        IEnumerable<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        var distinctTagIds = tagIds.Distinct().ToArray();
        if (distinctTagIds.Length == 0)
        {
            return [];
        }

        return await dbContext.Drinks
            .AsNoTracking()
            .SelectMany(drink => drink.Tags.Select(tag => tag.Id))
            .Where(tagId => distinctTagIds.Contains(tagId))
            .GroupBy(tagId => tagId)
            .Select(group => new { TagId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.TagId, entry => entry.Count, cancellationToken);
    }

    private async Task<Dictionary<Guid, IngredientReadModel>> LoadIngredientsAsync(
        IEnumerable<Guid> ingredientIds,
        CancellationToken cancellationToken)
    {
        var distinctIngredientIds = ingredientIds.Distinct().ToArray();
        if (distinctIngredientIds.Length == 0)
        {
            return [];
        }

        return await dbContext.Ingredients
            .AsNoTracking()
            .Where(ingredient => distinctIngredientIds.Contains(ingredient.Id))
            .Select(ingredient => new IngredientReadModel(
                ingredient.Id,
                ingredient.Name,
                ingredient.NotableBrands))
            .ToDictionaryAsync(ingredient => ingredient.Id, cancellationToken);
    }
}
