namespace AlCopilot.DrinkCatalog.Features.IngredientCategory;

public sealed class IngredientCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
