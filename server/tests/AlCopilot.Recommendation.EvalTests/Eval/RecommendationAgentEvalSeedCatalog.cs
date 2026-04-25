using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.Recommendation.EvalTests.Eval;

internal sealed class RecommendationAgentEvalSeedCatalog
{
    private RecommendationAgentEvalSeedCatalog(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        IReadOnlyDictionary<string, Guid> ingredients)
    {
        Drinks = drinks;
        Ingredients = ingredients;
    }

    public IReadOnlyCollection<DrinkDetailDto> Drinks { get; }

    public IReadOnlyDictionary<string, Guid> Ingredients { get; }

    public static RecommendationAgentEvalSeedCatalog Create()
    {
        var ingredientIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
        {
            ["White Rum"] = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            ["Dark Rum"] = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            ["Ginger Beer"] = Guid.Parse("10000000-0000-0000-0000-000000000003"),
            ["Lime Juice"] = Guid.Parse("10000000-0000-0000-0000-000000000004"),
            ["Simple Syrup"] = Guid.Parse("10000000-0000-0000-0000-000000000005"),
            ["Gin"] = Guid.Parse("10000000-0000-0000-0000-000000000006"),
            ["Campari"] = Guid.Parse("10000000-0000-0000-0000-000000000007"),
            ["Sweet Vermouth"] = Guid.Parse("10000000-0000-0000-0000-000000000008"),
            ["Soda Water"] = Guid.Parse("10000000-0000-0000-0000-000000000009"),
            ["Prosecco"] = Guid.Parse("10000000-0000-0000-0000-000000000010"),
            ["Bourbon"] = Guid.Parse("10000000-0000-0000-0000-000000000011"),
            ["Angostura Bitters"] = Guid.Parse("10000000-0000-0000-0000-000000000012"),
            ["Sugar Cube"] = Guid.Parse("10000000-0000-0000-0000-000000000013"),
        };

        var drinks = new List<DrinkDetailDto>
        {
            CreateDrink(
                "20000000-0000-0000-0000-000000000001",
                "Daiquiri",
                "Classic",
                "Strong, sweet, tart, and bright with clean rum character.",
                "Shake with ice and strain into a chilled coupe.",
                "Lime wheel",
                ingredientIds,
                [
                    ("White Rum", "2 oz", null),
                    ("Lime Juice", "1 oz", null),
                    ("Simple Syrup", "3/4 oz", null),
                ]),
            CreateDrink(
                "20000000-0000-0000-0000-000000000002",
                "Dark 'n Stormy",
                "Highball",
                "Spicy, refreshing, and rum-forward with ginger beer.",
                "Build over ice and gently stir.",
                "Lime wedge",
                ingredientIds,
                [
                    ("Dark Rum", "2 oz", null),
                    ("Ginger Beer", "4 oz", null),
                    ("Lime Juice", "1/2 oz", null),
                ]),
            CreateDrink(
                "20000000-0000-0000-0000-000000000003",
                "Negroni",
                "Classic",
                "Bittersweet, bold, and spirit-forward.",
                "Stir with ice and strain over fresh ice.",
                "Orange twist",
                ingredientIds,
                [
                    ("Gin", "1 oz", null),
                    ("Campari", "1 oz", null),
                    ("Sweet Vermouth", "1 oz", null),
                ]),
            CreateDrink(
                "20000000-0000-0000-0000-000000000004",
                "Americano",
                "Highball",
                "Light, bitter, sparkling, and gently sweet.",
                "Build over ice and top with soda.",
                "Orange slice",
                ingredientIds,
                [
                    ("Campari", "1 1/2 oz", null),
                    ("Sweet Vermouth", "1 1/2 oz", null),
                    ("Soda Water", "Top", null),
                ]),
            CreateDrink(
                "20000000-0000-0000-0000-000000000005",
                "French 75",
                "Sparkling",
                "Light, celebratory, citrusy, and sparkling.",
                "Shake gin, citrus, and syrup, then top with Prosecco.",
                "Lemon twist",
                ingredientIds,
                [
                    ("Gin", "1 oz", null),
                    ("Lime Juice", "1/2 oz", null),
                    ("Simple Syrup", "1/2 oz", null),
                    ("Prosecco", "3 oz", null),
                ]),
            CreateDrink(
                "20000000-0000-0000-0000-000000000006",
                "Old Fashioned",
                "Classic",
                "Strong, rich, and aromatic.",
                "Stir briefly over ice.",
                "Orange twist",
                ingredientIds,
                [
                    ("Bourbon", "2 oz", null),
                    ("Angostura Bitters", "2 dashes", null),
                    ("Sugar Cube", "1", null),
                ]),
        };

        return new RecommendationAgentEvalSeedCatalog(drinks, ingredientIds);
    }

    public Guid? FindIngredientId(string name)
    {
        return Ingredients.TryGetValue(name, out var ingredientId) ? ingredientId : null;
    }

    private static DrinkDetailDto CreateDrink(
        string id,
        string name,
        string? category,
        string description,
        string method,
        string garnish,
        IReadOnlyDictionary<string, Guid> ingredientIds,
        IReadOnlyCollection<(string IngredientName, string Quantity, string? RecommendedBrand)> recipeEntries)
    {
        return new DrinkDetailDto(
            Guid.Parse(id),
            name,
            category,
            description,
            method,
            garnish,
            null,
            [],
            recipeEntries
                .Select(entry => new RecipeEntryDto(
                    new IngredientDto(
                        ingredientIds[entry.IngredientName],
                        entry.IngredientName,
                        []),
                    entry.Quantity,
                    entry.RecommendedBrand))
                .ToList());
    }
}
