namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class RecipeEntry
{
    public Guid DrinkId { get; private set; }
    public Guid IngredientId { get; private set; }
    public Quantity Quantity { get; private set; } = null!;
    public string? RecommendedBrand { get; private set; }

    private RecipeEntry() { }

    public static RecipeEntry Create(Guid drinkId, Guid ingredientId, Quantity quantity, string? recommendedBrand)
    {
        return new RecipeEntry
        {
            DrinkId = drinkId,
            IngredientId = ingredientId,
            Quantity = quantity,
            RecommendedBrand = recommendedBrand
        };
    }
}
