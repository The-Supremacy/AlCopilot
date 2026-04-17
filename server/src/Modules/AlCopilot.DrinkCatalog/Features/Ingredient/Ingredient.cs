using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public sealed class Ingredient : AggregateRoot<Guid>
{
    public IngredientName Name { get; private set; } = null!;
    public List<string> NotableBrands { get; private set; } = [];
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Ingredient() { }

    public static Ingredient Create(IngredientName name, List<string>? notableBrands = null)
    {
        var ingredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = name,
            NotableBrands = notableBrands ?? [],
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        ingredient.Raise(new IngredientCreatedEvent(ingredient.Id));
        return ingredient;
    }

    public void Update(IngredientName name, List<string> brands)
    {
        Name = name;
        NotableBrands = brands;
        Raise(new IngredientUpdatedEvent(Id));
    }
}
