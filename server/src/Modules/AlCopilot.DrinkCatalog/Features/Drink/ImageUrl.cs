namespace AlCopilot.DrinkCatalog.Features.Drink;

public readonly record struct ImageUrl(string? Value)
{
    public static ImageUrl Create(string? value) => new(value);
}
