namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class RecipeEntry
{
    public Guid DrinkId { get; set; }
    public Guid IngredientId { get; set; }
    public string Quantity { get; set; } = string.Empty;
    public string? RecommendedBrand { get; set; }
}
