using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Domain.ValueObjects;

public sealed class DrinkName : ValueObject<string>
{
    private DrinkName(string value) : base(value) { }

    public static DrinkName Create(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Drink name cannot be empty.", nameof(value));
        if (trimmed.Length > 200)
            throw new ArgumentException("Drink name cannot exceed 200 characters.", nameof(value));
        return new DrinkName(trimmed);
    }
}
