using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Domain.ValueObjects;

public sealed class Quantity : ValueObject<string>
{
    private Quantity(string value) : base(value) { }

    public static Quantity Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Quantity cannot be empty.", nameof(value));
        if (value.Length > 100)
            throw new ArgumentException("Quantity cannot exceed 100 characters.", nameof(value));
        return new Quantity(value);
    }
}
