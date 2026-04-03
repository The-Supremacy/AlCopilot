namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public sealed class Ingredient
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid IngredientCategoryId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public List<string> NotableBrands { get; set; } = [];
}
