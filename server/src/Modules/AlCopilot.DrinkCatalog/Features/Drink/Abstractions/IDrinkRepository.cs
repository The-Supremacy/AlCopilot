using AlCopilot.Shared.Data;

namespace AlCopilot.DrinkCatalog.Features.Drink.Abstractions;

public interface IDrinkRepository : IRepository<Drink, Guid>
{
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Drink?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
