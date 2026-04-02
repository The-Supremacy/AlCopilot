using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Domain.ValueObjects;

public sealed class CategoryName : ValueObject<string>
{
    private CategoryName(string value) : base(value) { }

    public static CategoryName Create(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Category name cannot be empty.", nameof(value));
        if (trimmed.Length > 100)
            throw new ArgumentException("Category name cannot exceed 100 characters.", nameof(value));
        return new CategoryName(trimmed);
    }
}
