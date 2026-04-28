using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public sealed class IngredientGroup : ValueObject<string?>
{
    private IngredientGroup(string? value) : base(value) { }

    public static IngredientGroup Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new IngredientGroup(null);

        var trimmed = value.Trim();
        if (trimmed.Length > 100)
            throw new ArgumentException("Ingredient group cannot exceed 100 characters.", nameof(value));

        return new IngredientGroup(trimmed);
    }
}
