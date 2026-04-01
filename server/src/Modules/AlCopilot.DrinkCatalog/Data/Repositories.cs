using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Domain.Aggregates;
using AlCopilot.Shared.Data;

namespace AlCopilot.DrinkCatalog.Data;

public interface IDrinkRepository : IRepository<Drink, Guid>
{
    Task<PagedResult<DrinkDto>> GetPagedAsync(List<Guid>? tagIds, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<DrinkDto>> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<DrinkDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ITagRepository : IRepository<Tag, Guid>
{
    Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByDrinksAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}

public interface IIngredientRepository : IRepository<Ingredient, Guid>
{
    Task<List<IngredientDto>> GetAllAsync(Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}

public interface IIngredientCategoryRepository : IRepository<IngredientCategory, Guid>
{
    Task<List<IngredientCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
