using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Strategies;

public sealed class IbaCocktailsSnapshotImportSourceStrategyTests
{
    private readonly IbaCocktailsSnapshotImportSourceStrategy _strategy = new();

    [Fact]
    public async Task CreateImportAsync_WithEmbeddedSnapshot_LoadsAndNormalizesCatalogImport()
    {
        var result = await _strategy.CreateImportAsync(new ImportSourceStrategyRequest(
            string.Empty,
            ImportProvenance.Empty));

        result.SourceFingerprint.ShouldStartWith("sha256:");
        result.Import.Drinks.Count.ShouldBeGreaterThan(80);
        result.Import.Tags.ShouldBeEmpty();
        result.Import.Ingredients.ShouldContain(i => i.Name == "Vodka");
        result.Import.Drinks.ShouldContain(d => d.Name == "Bellini" && d.Category == "Contemporary Classics");
        result.Provenance.SourceReference.ShouldBe("seed/rasmusab/iba-cocktails/iba-web/iba-cocktails-web.snapshot.json");
        result.Provenance.Metadata["seedDataset"].ShouldBe("rasmusab/iba-cocktails");
    }

    [Fact]
    public async Task CreateImportAsync_WithSnapshotPayload_NormalizesDrinksIngredientsAndTags()
    {
        const string payload = """
        [
          {
            "category": "The Unforgettables",
            "name": "Negroni",
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
        result.Import.Ingredients.ShouldContain(i => i.Name == "Sweet Vermouth");
        result.Import.Drinks.ShouldHaveSingleItem().Category.ShouldBe("The Unforgettables");
        result.Import.Drinks.ShouldHaveSingleItem().RecipeEntries.Select(x => x.Quantity).ShouldBe(["30 ml", "30 ml", "30 ml"]);
    }
}
