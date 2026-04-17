using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AlCopilot.Shared.Errors;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;

internal sealed class IbaCocktailsSnapshotImportSourceStrategy : IImportSourceStrategy
{
    private const string EmbeddedSnapshotResourceName = "AlCopilot.DrinkCatalog.Features.ImportBatch.SeedData.iba-cocktails-web.snapshot.json";
    private const string SnapshotSourceReference = "seed/rasmusab/iba-cocktails/iba-web/iba-cocktails-web.snapshot.json";
    private const string SnapshotDisplayName = "iba-cocktails-web.snapshot.json";
    private const string SnapshotUpstreamRepository = "https://github.com/rasmusab/iba-cocktails";
    private const string SnapshotUpstreamPath = "iba-web/iba-cocktails-web.json";
    private const string SnapshotCommit = "9148d3302f582b06695684f3bb446631ab99d160";
    private const string SnapshotCapturedOn = "2026-04-11";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ImportStrategyKey Key => ImportStrategyKey.IbaCocktailsSnapshot;

    public ValueTask<ImportSourceStrategyResult> CreateImportAsync(
        ImportSourceStrategyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var payload = string.IsNullOrWhiteSpace(request.Payload)
            ? LoadEmbeddedSnapshotJson()
            : request.Payload.Trim();

        var cocktails = JsonSerializer.Deserialize<List<IbaCocktailPayload>>(payload, JsonSerializerOptions)
            ?? throw new ValidationException("Unable to deserialize IBA cocktails snapshot payload.");

        var normalizedImport = Normalize(cocktails);
        var provenance = BuildProvenance(request.Provenance, normalizedImport.Drinks.Count, normalizedImport.Ingredients.Count);

        return ValueTask.FromResult(new ImportSourceStrategyResult(
            provenance,
            normalizedImport,
            []));
    }

    private static string LoadEmbeddedSnapshotJson()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(EmbeddedSnapshotResourceName)
            ?? throw new InvalidOperationException($"Embedded snapshot resource '{EmbeddedSnapshotResourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static ImportProvenance BuildProvenance(ImportProvenance requestProvenance, int drinkCount, int ingredientCount)
    {
        var metadata = new Dictionary<string, string?>(requestProvenance.Metadata, StringComparer.OrdinalIgnoreCase)
        {
            ["format"] = "json",
            ["seedDataset"] = "rasmusab/iba-cocktails",
            ["seedDatasetSchema"] = "iba-cocktails-web.json",
            ["seedDatasetCommit"] = SnapshotCommit,
            ["seedDatasetCapturedOn"] = SnapshotCapturedOn,
            ["seedDatasetUpstreamRepository"] = SnapshotUpstreamRepository,
            ["seedDatasetUpstreamPath"] = SnapshotUpstreamPath,
            ["drinkCount"] = drinkCount.ToString(CultureInfo.InvariantCulture),
            ["ingredientCount"] = ingredientCount.ToString(CultureInfo.InvariantCulture),
        };

        return requestProvenance with
        {
            SourceReference = string.IsNullOrWhiteSpace(requestProvenance.SourceReference) ? SnapshotSourceReference : requestProvenance.SourceReference,
            DisplayName = string.IsNullOrWhiteSpace(requestProvenance.DisplayName) ? SnapshotDisplayName : requestProvenance.DisplayName,
            ContentType = "application/json",
            Metadata = metadata
        };
    }

    private static NormalizedCatalogImport Normalize(IEnumerable<IbaCocktailPayload> cocktails)
    {
        var ingredients = new Dictionary<string, NormalizedIngredientImport>(StringComparer.OrdinalIgnoreCase);
        var drinks = new List<NormalizedDrinkImport>();

        foreach (var cocktail in cocktails)
        {
            var drinkName = NormalizeRequired(cocktail.Name, nameof(cocktail.Name));
            var drinkCategory = NormalizeOptional(cocktail.Category);
            var method = NormalizeOptional(cocktail.Method);
            var garnish = NormalizeOptional(cocktail.Garnish);

            var recipeEntries = new List<NormalizedDrinkRecipeEntryImport>();
            foreach (var ingredientPayload in cocktail.Ingredients ?? [])
            {
                var incomingName = NormalizeIngredientName(NormalizeRequired(ingredientPayload.Ingredient, nameof(ingredientPayload.Ingredient)));
                if (!ingredients.TryGetValue(incomingName, out var existing))
                {
                    existing = new NormalizedIngredientImport(incomingName, []);
                    ingredients[incomingName] = existing;
                }

                recipeEntries.Add(new NormalizedDrinkRecipeEntryImport(
                    existing.Name,
                    BuildQuantity(ingredientPayload),
                    null));
            }

            drinks.Add(new NormalizedDrinkImport(
                drinkName,
                drinkCategory,
                null,
                method,
                garnish,
                null,
                [],
                recipeEntries));
        }

        return new NormalizedCatalogImport(
            [],
            ingredients.Values.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToList(),
            drinks.OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase).ToList());
    }

    private static string BuildQuantity(IbaIngredientPayload ingredientPayload)
    {
        var quantity = NormalizeOptional(ingredientPayload.Quantity);
        var unit = NormalizeOptional(ingredientPayload.Unit);
        var direction = NormalizeOptional(ingredientPayload.Direction);

        if (quantity is not null && unit is not null)
            return $"{quantity} {unit}";

        if (quantity is not null)
            return quantity;

        return direction ?? throw new ValidationException("Ingredient entry must contain quantity or direction.");
    }

    private static string NormalizeIngredientName(string value)
    {
        var normalized = NormalizeWhitespace(value.Normalize(NormalizationForm.FormKC))
            ?? throw new ValidationException("Ingredient name cannot be empty.");

        return CanonicalIngredientNames.TryGetValue(normalized, out var canonical)
            ? canonical
            : normalized;
    }



    private static string NormalizeRequired(string? value, string paramName)
    {
        var normalized = NormalizeOptional(value);
        if (normalized is null)
            throw new ValidationException($"Required value '{paramName}' is missing.");

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = NormalizeWhitespace(value?.Trim().Normalize(NormalizationForm.FormKC));
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private sealed record IbaCocktailPayload(
        string? Category,
        string? Name,
        string? Method,
        string? Garnish,
        List<IbaIngredientPayload>? Ingredients);

    private sealed record IbaIngredientPayload(
        string? Direction,
        string? Quantity,
        string? Unit,
        string? Ingredient);

    private static readonly Dictionary<string, string> CanonicalIngredientNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Angostura bitters"] = "Angostura Bitters",
        ["Bitter Campari"] = "Campari",
        ["Bénédictine"] = "Benedictine",
        ["DOM Bénédictine"] = "DOM Benedictine",
        ["Crème de Cassis"] = "Creme de Cassis",
        ["Crème de Cassis"] = "Creme de Cassis",
        ["Crème de Cacao"] = "Creme de Cacao",
        ["Crème de Menthe"] = "Creme de Menthe",
        ["Crème de Mûre"] = "Creme de Mure",
        ["Crème de Violette"] = "Creme de Violette",
        ["Fresh lemon juice"] = "Lemon Juice",
        ["Fresh lemon Juice"] = "Lemon Juice",
        ["Fresh Lemon Juice"] = "Lemon Juice",
        ["Lemon juice"] = "Lemon Juice",
        ["Fresh lime juice"] = "Lime Juice",
        ["Fresh Lime Juice"] = "Lime Juice",
        ["Freshly Squeezed Lime Juice"] = "Lime Juice",
        ["Lime Juice"] = "Lime Juice",
        ["Fresh lime"] = "Lime",
        ["Fresh Lime"] = "Lime",
        ["Fresh orange juice"] = "Orange Juice",
        ["Fresh Orange Juice"] = "Orange Juice",
        ["Fresh Pineapple Juice"] = "Pineapple Juice",
        ["Ginger beer"] = "Ginger Beer",
        ["Soda water"] = "Soda Water",
        ["strong Espresso"] = "Espresso",
        ["Simple syrup"] = "Simple Syrup",
        ["Sugar syrup"] = "Simple Syrup",
        ["Sugar Syrup"] = "Simple Syrup",
        ["White rum"] = "White Rum",
        ["fresh Mint sprigs"] = "Mint Sprigs",
        ["Cognac or Brandy"] = "Cognac Or Brandy",
        ["Bourbon or Rye Whiskey"] = "Bourbon Or Rye Whiskey",
        ["Rye Whiskey or Bourbon"] = "Bourbon Or Rye Whiskey",
        ["Tequila 100% Agave"] = "100% Agave Tequila",
        ["Tequila Agave 100% Reposado"] = "100% Agave Reposado Tequila",
        ["Smirnoff Vodka"] = "Vodka",
        ["Chilled Champagne"] = "Champagne",
        ["Brut Champagne or Prosecco"] = "Brut Champagne Or Prosecco",
    };

}
