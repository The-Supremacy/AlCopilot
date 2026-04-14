using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using DrinkAggregate = AlCopilot.DrinkCatalog.Features.Drink.Drink;
using DrinkImageUrl = AlCopilot.DrinkCatalog.Features.Drink.ImageUrl;
using DrinkNameValue = AlCopilot.DrinkCatalog.Features.Drink.DrinkName;
using DrinkQuantity = AlCopilot.DrinkCatalog.Features.Drink.Quantity;
using RecipeEntryEntity = AlCopilot.DrinkCatalog.Features.Drink.RecipeEntry;
using IngredientAggregate = AlCopilot.DrinkCatalog.Features.Ingredient.Ingredient;
using IngredientNameValue = AlCopilot.DrinkCatalog.Features.Ingredient.IngredientName;
using TagAggregate = AlCopilot.DrinkCatalog.Features.Tag.Tag;
using TagNameValue = AlCopilot.DrinkCatalog.Features.Tag.TagName;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Errors;
using AlCopilot.Shared.Models;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed class ImportBatchWorkflowService(
    ITagRepository tagRepository,
    IIngredientRepository ingredientRepository,
    IDrinkRepository drinkRepository,
    IDrinkQueryService drinkQueryService)
{
    public async Task<List<ImportDiagnostic>> ValidateAsync(
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

    public async Task<ImportReviewResult> ReviewAsync(
        NormalizedCatalogImport import,
        IReadOnlyCollection<ImportDiagnostic>? diagnostics,
        CancellationToken cancellationToken)
    {
        var createCount = 0;
        var updateCount = 0;
        var skipCount = 0;
        var conflicts = new List<ImportReviewConflict>();
        var rows = new List<ImportReviewRow>();
        var batchDiagnostics = diagnostics ?? [];

        foreach (var tag in import.Tags)
        {
            if (await tagRepository.GetByNameAsync(tag.Name, cancellationToken) is null)
            {
                createCount++;
                rows.Add(new ImportReviewRow("tag", tag.Name, "create", $"Create tag '{tag.Name}'.", false, HasError(batchDiagnostics, "tag", tag.Name)));
            }
            else
            {
                skipCount++;
                rows.Add(new ImportReviewRow("tag", tag.Name, "skip", $"Tag '{tag.Name}' already exists.", false, HasError(batchDiagnostics, "tag", tag.Name)));
            }
        }

        foreach (var ingredient in import.Ingredients)
        {
            var existing = await ingredientRepository.GetByNameAsync(ingredient.Name, cancellationToken);
            if (existing is null)
            {
                createCount++;
                rows.Add(new ImportReviewRow(
                    "ingredient",
                    ingredient.Name,
                    "create",
                    $"Create ingredient '{ingredient.Name}'.",
                    false,
                    HasError(batchDiagnostics, "ingredient", ingredient.Name)));
                continue;
            }
            var brandsChanged = !existing.NotableBrands
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .SequenceEqual(ingredient.NotableBrands.OrderBy(x => x, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
            if (brandsChanged)
            {
                updateCount++;
                var conflict = new ImportReviewConflict(
                    "ingredient",
                    ingredient.Name,
                    "update",
                    $"Ingredient '{ingredient.Name}' would update notable brands.");
                conflicts.Add(conflict);
                rows.Add(new ImportReviewRow(
                    conflict.TargetType,
                    conflict.TargetKey,
                    conflict.Action,
                    conflict.Summary,
                    true,
                    HasError(batchDiagnostics, "ingredient", ingredient.Name)));
            }
            else
            {
                skipCount++;
                rows.Add(new ImportReviewRow(
                    "ingredient",
                    ingredient.Name,
                    "skip",
                    $"Ingredient '{ingredient.Name}' is unchanged.",
                    false,
                    HasError(batchDiagnostics, "ingredient", ingredient.Name)));
            }
        }

        var existingDrinks = await drinkQueryService.GetAllAsync(cancellationToken);
        var existingDrinksByName = existingDrinks.ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var drink in import.Drinks)
        {
            if (!existingDrinksByName.TryGetValue(drink.Name, out var existing))
            {
                createCount++;
                rows.Add(new ImportReviewRow(
                    "drink",
                    drink.Name,
                    "create",
                    $"Create drink '{drink.Name}'.",
                    false,
                    HasError(batchDiagnostics, "drink", drink.Name)));
                continue;
            }

            if (DrinksDiffer(existing, drink))
            {
                updateCount++;
                var conflict = new ImportReviewConflict(
                    "drink",
                    drink.Name,
                    "update",
                    $"Drink '{drink.Name}' would update metadata, tags, or recipe entries.");
                conflicts.Add(conflict);
                rows.Add(new ImportReviewRow(
                    conflict.TargetType,
                    conflict.TargetKey,
                    conflict.Action,
                    conflict.Summary,
                    true,
                    HasError(batchDiagnostics, "drink", drink.Name)));
            }
            else
            {
                skipCount++;
                rows.Add(new ImportReviewRow(
                    "drink",
                    drink.Name,
                    "skip",
                    $"Drink '{drink.Name}' is unchanged.",
                    false,
                    HasError(batchDiagnostics, "drink", drink.Name)));
            }
        }

        return new ImportReviewResult(
            new ImportReviewSummary(createCount, updateCount, skipCount),
            conflicts,
            rows);
    }

    public async Task<ImportApplySummary> ApplyAsync(
        ImportBatch batch,
        IReadOnlyDictionary<string, ImportDecisionInput> decisions,
        CurrentActor currentActor,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;
        var skipped = 0;
        var rejected = 0;
        var audit = new List<ImportDecisionAuditEntry>();

        foreach (var tagImport in batch.ImportContent.Tags)
        {
            if (await tagRepository.GetByNameAsync(tagImport.Name, cancellationToken) is null)
            {
                tagRepository.Add(TagAggregate.Create(TagNameValue.Create(tagImport.Name)));
                created++;
            }
            else
            {
                skipped++;
            }
        }

        foreach (var ingredientImport in batch.ImportContent.Ingredients)
        {
            var existing = await ingredientRepository.GetByNameAsync(ingredientImport.Name, cancellationToken);

            if (existing is null)
            {
                ingredientRepository.Add(Ingredient.Ingredient.Create(
                    IngredientNameValue.Create(ingredientImport.Name),
                    ingredientImport.NotableBrands));
                created++;
                continue;
            }

            var conflictKey = BuildDecisionKey("ingredient", ingredientImport.Name);
            var isConflict = batch.ReviewConflicts.Any(c => BuildDecisionKey(c.TargetType, c.TargetKey) == conflictKey);

            if (!isConflict)
            {
                skipped++;
                continue;
            }

            var decision = GetRequiredDecision(decisions, "ingredient", ingredientImport.Name);
            if (IsApproveDecision(decision.Decision))
            {
                existing.Update(IngredientNameValue.Create(ingredientImport.Name), ingredientImport.NotableBrands);
                updated++;
                audit.Add(new ImportDecisionAuditEntry("ingredient", ingredientImport.Name, decision.Decision, decision.Reason, currentActor.UserId, currentActor.DisplayName, DateTimeOffset.UtcNow));
            }
            else
            {
                rejected++;
                audit.Add(new ImportDecisionAuditEntry("ingredient", ingredientImport.Name, decision.Decision, decision.Reason, currentActor.UserId, currentActor.DisplayName, DateTimeOffset.UtcNow));
            }
        }

        foreach (var drinkImport in batch.ImportContent.Drinks)
        {
            var existing = await drinkRepository.GetByNameAsync(drinkImport.Name, cancellationToken);
            var tags = await ResolveTagsAsync(drinkImport.TagNames, cancellationToken);
            var recipeEntries = await ResolveRecipeEntriesAsync(existing?.Id ?? Guid.NewGuid(), drinkImport.RecipeEntries, cancellationToken);

            if (existing is null)
            {
                var drink = DrinkAggregate.Create(
                    DrinkNameValue.Create(drinkImport.Name),
                    DrinkCategory.Create(drinkImport.Category),
                    drinkImport.Description,
                    drinkImport.Method,
                    drinkImport.Garnish,
                    DrinkImageUrl.Create(drinkImport.ImageUrl));
                drink.SetTags(tags);
                drink.SetRecipeEntries(await ResolveRecipeEntriesAsync(drink.Id, drinkImport.RecipeEntries, cancellationToken));
                drinkRepository.Add(drink);
                created++;
                continue;
            }

            var conflictKey = BuildDecisionKey("drink", drinkImport.Name);
            var isConflict = batch.ReviewConflicts.Any(c => BuildDecisionKey(c.TargetType, c.TargetKey) == conflictKey);

            if (!isConflict)
            {
                skipped++;
                continue;
            }

            var decision = GetRequiredDecision(decisions, "drink", drinkImport.Name);
            if (IsApproveDecision(decision.Decision))
            {
                existing.Update(
                    DrinkNameValue.Create(drinkImport.Name),
                    DrinkCategory.Create(drinkImport.Category),
                    drinkImport.Description,
                    drinkImport.Method,
                    drinkImport.Garnish,
                    DrinkImageUrl.Create(drinkImport.ImageUrl));
                existing.SetTags(tags);
                existing.SetRecipeEntries(await ResolveRecipeEntriesAsync(existing.Id, drinkImport.RecipeEntries, cancellationToken));
                updated++;
                audit.Add(new ImportDecisionAuditEntry("drink", drinkImport.Name, decision.Decision, decision.Reason, currentActor.UserId, currentActor.DisplayName, DateTimeOffset.UtcNow));
            }
            else
            {
                rejected++;
                audit.Add(new ImportDecisionAuditEntry("drink", drinkImport.Name, decision.Decision, decision.Reason, currentActor.UserId, currentActor.DisplayName, DateTimeOffset.UtcNow));
            }
        }

        batch.MarkCompleted(new ImportApplySummary(created, updated, skipped, rejected), audit);
        return batch.ApplySummary!;
    }

    public static string BuildDecisionKey(string targetType, string targetKey) =>
        $"{targetType.Trim().ToLowerInvariant()}::{targetKey.Trim().ToLowerInvariant()}";

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

    private async Task<List<TagAggregate>> ResolveTagsAsync(IEnumerable<string> tagNames, CancellationToken cancellationToken)
    {
        var tags = new List<TagAggregate>();
        foreach (var tagName in tagNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var tag = await tagRepository.GetByNameAsync(tagName, cancellationToken)
                ?? throw new InvalidStateException($"Tag '{tagName}' not found during apply.");
            tags.Add(tag);
        }

        return tags;
    }

    private async Task<List<RecipeEntryEntity>> ResolveRecipeEntriesAsync(
        Guid drinkId,
        IEnumerable<NormalizedDrinkRecipeEntryImport> entries,
        CancellationToken cancellationToken)
    {
        var recipeEntries = new List<RecipeEntryEntity>();
        foreach (var entry in entries)
        {
            var ingredient = await ingredientRepository.GetByNameAsync(entry.IngredientName, cancellationToken)
                ?? throw new InvalidStateException($"Ingredient '{entry.IngredientName}' not found during apply.");

            recipeEntries.Add(RecipeEntryEntity.Create(
                drinkId,
                ingredient.Id,
                DrinkQuantity.Create(entry.Quantity),
                entry.RecommendedBrand));
        }

        return recipeEntries;
    }

    private static ImportDecisionInput GetRequiredDecision(
        IReadOnlyDictionary<string, ImportDecisionInput> decisions,
        string targetType,
        string targetKey)
    {
        var key = BuildDecisionKey(targetType, targetKey);
        if (decisions.TryGetValue(key, out var decision))
            return decision;

        throw new InvalidStateException(
            $"Conflict '{targetType}:{targetKey}' requires an explicit decision before apply.");
    }

    private static bool IsApproveDecision(string decision) =>
        string.Equals(decision, "approve-update", StringComparison.OrdinalIgnoreCase)
        || string.Equals(decision, "approve", StringComparison.OrdinalIgnoreCase);

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
