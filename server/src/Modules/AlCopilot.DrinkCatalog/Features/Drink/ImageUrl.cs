using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class ImageUrl : ValueObject<string?>
{
    private ImageUrl(string? value) : base(value) { }

    public static ImageUrl Create(string? value)
    {
        if (value is not null && value.Length > 1000)
            throw new ArgumentException("Image URL cannot exceed 1000 characters.", nameof(value));
        return new ImageUrl(value);
    }
}
