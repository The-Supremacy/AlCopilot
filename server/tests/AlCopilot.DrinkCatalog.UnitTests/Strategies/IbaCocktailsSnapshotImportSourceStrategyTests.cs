using AlCopilot.DrinkCatalog.Features.ImportBatch;
using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;
using Shouldly;

namespace AlCopilot.DrinkCatalog.UnitTests.Strategies;

public sealed class IbaCocktailsSnapshotImportSourceStrategyTests
{
    private readonly IbaCocktailsSnapshotImportSourceStrategy _strategy = new();

    [Fact]
    public async Task CreateImportAsync_WithEmbeddedSnapshot_LoadsAndNormalizesCatalogImport()
    {
        var result = await _strategy.CreateImportAsync(new ImportSourceStrategyRequest(
            string.Empty,
            ImportProvenance.Empty));

        result.Import.Drinks.Count.ShouldBeGreaterThan(80);
        result.Import.Tags.ShouldBeEmpty();
        result.Import.Ingredients.ShouldContain(i => i.Name == "Vodka");
        result.Import.Ingredients.ShouldContain(i => i.Name == "Gin" && i.IngredientGroup == "Gin");
        result.Import.Ingredients.ShouldContain(i => i.Name == "White Rum" && i.IngredientGroup == "Rum");
        result.Import.Drinks.ShouldContain(d => d.Name == "Bellini" && d.Category == "Contemporary Classics");
        result.Import.Drinks.ShouldContain(d =>
            d.Name == "Bellini"
            && d.Description == "A light sparkling aperitif that pairs juicy white peach with dry prosecco for a soft, fruity finish.");
        result.Provenance.SourceReference.ShouldBe("seed/alcopilot/iba-cocktails/iba-web/iba-cocktails-web.extended.snapshot.json");
        result.Provenance.DisplayName.ShouldBe("iba-cocktails-web.extended.snapshot.json");
        result.Provenance.Metadata["seedDataset"].ShouldBe("rasmusab/iba-cocktails");
        result.Provenance.Metadata["seedDatasetVariant"].ShouldBe("alcopilot-extended");
        result.Provenance.Metadata["seedDatasetCuratedFields"].ShouldBe("description");
    }

    [Fact]
    public async Task CreateImportAsync_WithSnapshotPayload_NormalizesDrinksIngredientsTagsAndDescriptions()
    {
        const string payload = """
        [
          {
            "category": "The Unforgettables",
            "name": "Negroni",
            "description": "A bittersweet stirred aperitif built on equal parts gin, bitter liqueur, and sweet vermouth.",
            "method": "Stir over ice and strain.",
            "ingredients": [
              { "direction": "30 ml Gin", "quantity": "30", "unit": "ml", "ingredient": "Gin" },
              { "direction": "30 ml Bitter Campari", "quantity": "30", "unit": "ml", "ingredient": "Bitter Campari" },
              { "direction": "30 ml Sweet Vermouth", "quantity": "30", "unit": "ml", "ingredient": "Sweet Vermouth" }
            ]
          }
        ]
        """;

        var result = await _strategy.CreateImportAsync(new ImportSourceStrategyRequest(
            payload,
            new ImportProvenance(null, null, "application/json", [])));

        result.Import.Tags.ShouldBeEmpty();
        result.Import.Ingredients.ShouldContain(i => i.Name == "Campari");
        result.Import.Ingredients.ShouldContain(i => i.Name == "Sweet Vermouth" && i.IngredientGroup == "Vermouth");
        result.Import.Drinks.ShouldHaveSingleItem().Category.ShouldBe("The Unforgettables");
        result.Import.Drinks.ShouldHaveSingleItem().Description.ShouldBe(
            "A bittersweet stirred aperitif built on equal parts gin, bitter liqueur, and sweet vermouth.");
        result.Import.Drinks.ShouldHaveSingleItem().RecipeEntries.Select(x => x.Quantity).ShouldBe(["30 ml", "30 ml", "30 ml"]);
    }
}
