using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;
using IngredientAggregate = AlCopilot.DrinkCatalog.Features.Ingredient.Ingredient;
using AlCopilot.DrinkCatalog.Features.Ingredient;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed class ImportBatchProcessingService(
    ITagRepository tagRepository,
    IIngredientRepository ingredientRepository,
    IDrinkQueryService drinkQueryService) : IImportBatchProcessingService
{
    public async Task<ImportBatchProcessingResult> ProcessAsync(
        NormalizedCatalogImport import,
        CancellationToken cancellationToken)
    {
        var diagnostics = await BuildDiagnosticsAsync(import, cancellationToken);
        var rows = new List<ImportReviewRow>();

        await AppendTagReviewRowsAsync(import, diagnostics, rows, cancellationToken);
        await AppendIngredientReviewRowsAsync(import, diagnostics, rows, cancellationToken);
        await AppendDrinkReviewRowsAsync(import, diagnostics, rows, cancellationToken);

        return new ImportBatchProcessingResult(
            diagnostics,
            BuildReviewSummary(rows),
            rows);
    }

    public ImportBatchApplyReadiness GetBatchApplyReadiness(ImportBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        return batch.GetApplyReadiness();
    }

    private static void AddDuplicateDiagnostics(
        List<ImportDiagnostic> diagnostics,
        IEnumerable<string> names,
        string code,
        string label)
    {
        foreach (var duplicate in names
                     .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1)
                     .Select(g => g.Key))
        {
            diagnostics.Add(new ImportDiagnostic(null, code, $"Duplicate {label} '{duplicate}' in import payload.", "warning"));
        }
    }

    private async Task<List<ImportDiagnostic>> BuildDiagnosticsAsync(
        NormalizedCatalogImport import,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<ImportDiagnostic>();

        AddDuplicateDiagnostics(diagnostics, import.Tags.Select(t => t.Name), "tag-name-duplicate", "tag");
        AddDuplicateDiagnostics(diagnostics, import.Ingredients.Select(i => i.Name), "ingredient-name-duplicate", "ingredient");
        AddDuplicateDiagnostics(diagnostics, import.Drinks.Select(d => d.Name), "drink-name-duplicate", "drink");

        var importIngredientNames = import.Ingredients
            .Select(i => i.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var drink in import.Drinks)
        {
            foreach (var recipeEntry in drink.RecipeEntries)
            {
                if (!importIngredientNames.Contains(recipeEntry.IngredientName)
                    && await ingredientRepository.GetByNameAsync(recipeEntry.IngredientName, cancellationToken) is null)
                {
                    diagnostics.Add(new ImportDiagnostic(
                        null,
                        "recipe-ingredient-missing",
                        $"Drink '{drink.Name}' references unresolvable ingredient '{recipeEntry.IngredientName}'.",
                        "error"));
                }
            }
        }

        return diagnostics;
    }

    private async Task AppendTagReviewRowsAsync(
        NormalizedCatalogImport import,
        IReadOnlyCollection<ImportDiagnostic> diagnostics,
        List<ImportReviewRow> rows,
        CancellationToken cancellationToken)
    {
        foreach (var tag in import.Tags)
        {
            if (await tagRepository.GetByNameAsync(tag.Name, cancellationToken) is null)
            {
                rows.Add(new ImportReviewRow("tag", tag.Name, "create", $"Create tag '{tag.Name}'.", false, HasError(diagnostics, "tag", tag.Name)));
                continue;
            }

            rows.Add(new ImportReviewRow("tag", tag.Name, "skip", $"Tag '{tag.Name}' already exists.", false, HasError(diagnostics, "tag", tag.Name)));
        }
    }

    private async Task AppendIngredientReviewRowsAsync(
        NormalizedCatalogImport import,
        IReadOnlyCollection<ImportDiagnostic> diagnostics,
        List<ImportReviewRow> rows,
        CancellationToken cancellationToken)
    {
        foreach (var ingredient in import.Ingredients)
        {
            var existing = await ingredientRepository.GetByNameAsync(ingredient.Name, cancellationToken);
            if (existing is null)
            {
                rows.Add(new ImportReviewRow(
                    "ingredient",
                    ingredient.Name,
                    "create",
                    $"Create ingredient '{ingredient.Name}'.",
                    false,
                    HasError(diagnostics, "ingredient", ingredient.Name)));
                continue;
            }

            rows.Add(CreateIngredientReviewRow(existing, ingredient, diagnostics));
        }
    }

    private async Task AppendDrinkReviewRowsAsync(
        NormalizedCatalogImport import,
        IReadOnlyCollection<ImportDiagnostic> diagnostics,
        List<ImportReviewRow> rows,
        CancellationToken cancellationToken)
    {
        var existingDrinks = await drinkQueryService.GetAllAsync(cancellationToken);
        var existingDrinksByName = existingDrinks.ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var drink in import.Drinks)
        {
            if (!existingDrinksByName.TryGetValue(drink.Name, out var existing))
            {
                rows.Add(new ImportReviewRow(
                    "drink",
                    drink.Name,
                    "create",
                    $"Create drink '{drink.Name}'.",
                    false,
                    HasError(diagnostics, "drink", drink.Name)));
                continue;
            }

            rows.Add(CreateDrinkReviewRow(existing, drink, diagnostics));
        }
    }

    private static ImportReviewSummary BuildReviewSummary(IReadOnlyCollection<ImportReviewRow> rows)
    {
        return new ImportReviewSummary(
            rows.Count(row => string.Equals(row.Action, "create", StringComparison.OrdinalIgnoreCase)),
            rows.Count(row => string.Equals(row.Action, "update", StringComparison.OrdinalIgnoreCase)),
            rows.Count(row => string.Equals(row.Action, "skip", StringComparison.OrdinalIgnoreCase)));
    }

    private static ImportReviewRow CreateIngredientReviewRow(
        IngredientAggregate existing,
        NormalizedIngredientImport ingredient,
        IReadOnlyCollection<ImportDiagnostic> diagnostics)
    {
        var brandsChanged = !existing.NotableBrands
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .SequenceEqual(ingredient.NotableBrands.OrderBy(x => x, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        var existingGroup = existing.GetGroupName();
        var groupChanged = !string.Equals(existingGroup, ingredient.IngredientGroup, StringComparison.Ordinal);

        if (brandsChanged || groupChanged)
        {
            return new ImportReviewRow(
                "ingredient",
                ingredient.Name,
                "update",
                $"Ingredient '{ingredient.Name}' would update notable brands or group.",
                true,
                HasError(diagnostics, "ingredient", ingredient.Name));
        }

        return new ImportReviewRow(
            "ingredient",
            ingredient.Name,
            "skip",
            $"Ingredient '{ingredient.Name}' is unchanged.",
            false,
            HasError(diagnostics, "ingredient", ingredient.Name));
    }

    private static ImportReviewRow CreateDrinkReviewRow(
        Contracts.DTOs.DrinkDetailDto existing,
        NormalizedDrinkImport drink,
        IReadOnlyCollection<ImportDiagnostic> diagnostics)
    {
        if (DrinksDiffer(existing, drink))
        {
            return new ImportReviewRow(
                "drink",
                drink.Name,
                "update",
                $"Drink '{drink.Name}' would update metadata, tags, or recipe entries.",
                true,
                HasError(diagnostics, "drink", drink.Name));
        }

        return new ImportReviewRow(
            "drink",
            drink.Name,
            "skip",
            $"Drink '{drink.Name}' is unchanged.",
            false,
            HasError(diagnostics, "drink", drink.Name));
    }

    private static bool DrinksDiffer(Contracts.DTOs.DrinkDetailDto existing, NormalizedDrinkImport incoming)
    {
        if (!string.Equals(existing.Category, incoming.Category, StringComparison.Ordinal))
            return true;
        if (!string.Equals(existing.Description, incoming.Description, StringComparison.Ordinal))
            return true;
        if (!string.Equals(existing.Method, incoming.Method, StringComparison.Ordinal))
            return true;
        if (!string.Equals(existing.Garnish, incoming.Garnish, StringComparison.Ordinal))
            return true;
        if (!string.Equals(existing.ImageUrl, incoming.ImageUrl, StringComparison.Ordinal))
            return true;

        var existingTags = existing.Tags.Select(t => t.Name).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
        var incomingTags = incoming.TagNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
        if (!existingTags.SequenceEqual(incomingTags, StringComparer.OrdinalIgnoreCase))
            return true;

        var existingRecipe = existing.RecipeEntries
            .Select(re => $"{re.Ingredient.Name}|{re.Quantity}|{re.RecommendedBrand}")
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
        var incomingRecipe = incoming.RecipeEntries
            .Select(re => $"{re.IngredientName}|{re.Quantity}|{re.RecommendedBrand}")
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase);

        return !existingRecipe.SequenceEqual(incomingRecipe, StringComparer.OrdinalIgnoreCase);
    }


    private static bool HasError(
        IReadOnlyCollection<ImportDiagnostic> diagnostics,
        string targetType,
        string targetKey)
    {
        return diagnostics.Any(d =>
            string.Equals(d.Severity, "error", StringComparison.OrdinalIgnoreCase)
            && MatchesDiagnosticTarget(d, targetType, targetKey));
    }

    private static bool MatchesDiagnosticTarget(
        ImportDiagnostic diagnostic,
        string targetType,
        string targetKey)
    {
        if (!diagnostic.Message.Contains($"'{targetKey}'", StringComparison.OrdinalIgnoreCase))
            return false;

        return targetType switch
        {
            "drink" => diagnostic.Message.Contains("Drink ", StringComparison.OrdinalIgnoreCase)
                || diagnostic.Code.Contains("drink", StringComparison.OrdinalIgnoreCase)
                || diagnostic.Code.Contains("recipe", StringComparison.OrdinalIgnoreCase),
            "ingredient" => diagnostic.Message.Contains("Ingredient ", StringComparison.OrdinalIgnoreCase)
                || diagnostic.Code.Contains("ingredient", StringComparison.OrdinalIgnoreCase),
            "tag" => diagnostic.Message.Contains("Tag ", StringComparison.OrdinalIgnoreCase)
                || diagnostic.Code.Contains("tag", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

}
