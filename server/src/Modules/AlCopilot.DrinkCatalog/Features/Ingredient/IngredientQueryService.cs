using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

internal sealed class IngredientQueryService(DrinkCatalogDbContext dbContext) : IIngredientQueryService
{
    public async Task<List<IngredientDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var ingredients = await dbContext.Ingredients
            .AsNoTracking()
            .OrderBy(ingredient => ingredient.Name)
            .Select(ingredient => new IngredientReadModel(
                ingredient.Id,
                ingredient.Name,
                ingredient.Group,
                ingredient.NotableBrands))
            .ToListAsync(cancellationToken);

        return ingredients.Select(ToDto).ToList();
    }

    private sealed record IngredientReadModel(
        Guid Id,
        string Name,
        string? IngredientGroup,
        List<string> NotableBrands);

    private static IngredientDto ToDto(IngredientReadModel ingredient)
    {
        return new IngredientDto(
            ingredient.Id,
            ingredient.Name,
            ingredient.NotableBrands,
            ingredient.IngredientGroup);
    }
}
