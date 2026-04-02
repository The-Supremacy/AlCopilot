using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Domain.ValueObjects;

public sealed class TagName : ValueObject<string>
{
    private TagName(string value) : base(value) { }

    public static TagName Create(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Tag name cannot be empty.", nameof(value));
        if (trimmed.Length > 100)
            throw new ArgumentException("Tag name cannot exceed 100 characters.", nameof(value));
        return new TagName(trimmed);
    }
}
