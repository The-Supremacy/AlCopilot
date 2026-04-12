using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class DrinkCategory : ValueObject<string?>
{
    private DrinkCategory(string? value) : base(value) { }

    public static DrinkCategory Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new DrinkCategory(null);

        value = value.Trim();
        if (value.Length > 100)
            throw new ArgumentException("Drink category cannot exceed 100 characters.", nameof(value));

        return new DrinkCategory(value);
    }
}
