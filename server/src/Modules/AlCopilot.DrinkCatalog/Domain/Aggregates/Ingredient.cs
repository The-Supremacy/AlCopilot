using AlCopilot.DrinkCatalog.Domain.ValueObjects;
using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Domain.Aggregates;

public sealed class Ingredient : AggregateRoot<Guid>
{
    public IngredientName Name { get; private set; } = null!;
    public Guid IngredientCategoryId { get; private set; }
    public List<string> NotableBrands { get; private set; } = [];
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Ingredient() { }

    public static Ingredient Create(IngredientName name, Guid ingredientCategoryId, List<string>? notableBrands = null)
    {
        return new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = name,
            IngredientCategoryId = ingredientCategoryId,
            NotableBrands = notableBrands ?? [],
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void UpdateBrands(List<string> brands)
    {
        NotableBrands = brands;
    }
}
