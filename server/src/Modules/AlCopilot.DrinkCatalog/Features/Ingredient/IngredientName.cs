using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public sealed class IngredientName : ValueObject<string>
{
    private IngredientName(string value) : base(value) { }

    public static IngredientName Create(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Ingredient name cannot be empty.", nameof(value));
        if (trimmed.Length > 200)
            throw new ArgumentException("Ingredient name cannot exceed 200 characters.", nameof(value));
        return new IngredientName(trimmed);
    }
}
