namespace AlCopilot.DrinkCatalog.Features.Drink;

public readonly record struct DrinkName(string Value)
{
    public static DrinkName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Drink name is required.", nameof(value));
        }

        return new DrinkName(value.Trim());
    }
}
