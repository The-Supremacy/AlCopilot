using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.IngredientCategory;

public sealed class IngredientCategory : AggregateRoot<Guid>
{
    public CategoryName Name { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private IngredientCategory() { }

    public static IngredientCategory Create(CategoryName name)
    {
        return new IngredientCategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
