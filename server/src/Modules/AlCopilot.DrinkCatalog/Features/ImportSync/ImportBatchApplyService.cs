using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using DrinkAggregate = AlCopilot.DrinkCatalog.Features.Drink.Drink;
using DrinkImageUrl = AlCopilot.DrinkCatalog.Features.Drink.ImageUrl;
using DrinkNameValue = AlCopilot.DrinkCatalog.Features.Drink.DrinkName;
using DrinkQuantity = AlCopilot.DrinkCatalog.Features.Drink.Quantity;
using IngredientNameValue = AlCopilot.DrinkCatalog.Features.Ingredient.IngredientName;
using RecipeEntryEntity = AlCopilot.DrinkCatalog.Features.Drink.RecipeEntry;
using TagAggregate = AlCopilot.DrinkCatalog.Features.Tag.Tag;
using TagNameValue = AlCopilot.DrinkCatalog.Features.Tag.TagName;
using AlCopilot.Shared.Errors;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed class ImportBatchApplyService(
    ITagRepository tagRepository,
    IIngredientRepository ingredientRepository,
    IDrinkRepository drinkRepository) : IImportBatchApplyService
{
    public async Task<ImportApplySummary> ApplyAsync(
        ImportBatch batch,
        CancellationToken cancellationToken)
    {
        var applyState = new ImportApplyState();

        await ApplyTagsAsync(batch, applyState, cancellationToken);
        await ApplyIngredientsAsync(batch, applyState, cancellationToken);
        await ApplyDrinksAsync(batch, applyState, cancellationToken);

        batch.MarkCompleted(applyState.ToSummary());
        return batch.ApplySummary!;
    }

    private async Task ApplyTagsAsync(
        ImportBatch batch,
        ImportApplyState applyState,
        CancellationToken cancellationToken)
    {
        foreach (var tagImport in batch.ImportContent.Tags)
        {
            if (await tagRepository.GetByNameAsync(tagImport.Name, cancellationToken) is null)
            {
                tagRepository.Add(TagAggregate.Create(TagNameValue.Create(tagImport.Name)));
                applyState.Created++;
                continue;
            }

            applyState.Skipped++;
        }
    }

    private async Task ApplyIngredientsAsync(
        ImportBatch batch,
        ImportApplyState applyState,
        CancellationToken cancellationToken)
    {
        foreach (var ingredientImport in batch.ImportContent.Ingredients)
        {
            var existing = await ingredientRepository.GetByNameAsync(ingredientImport.Name, cancellationToken);

            if (existing is null)
            {
                ingredientRepository.Add(Ingredient.Ingredient.Create(
                    IngredientNameValue.Create(ingredientImport.Name),
                    ingredientImport.NotableBrands));
                applyState.Created++;
                continue;
            }

            if (RequiresReviewedUpdate(batch, "ingredient", ingredientImport.Name))
            {
                existing.Update(IngredientNameValue.Create(ingredientImport.Name), ingredientImport.NotableBrands);
                applyState.Updated++;
                continue;
            }

            applyState.Skipped++;
        }
    }

    private async Task ApplyDrinksAsync(
        ImportBatch batch,
        ImportApplyState applyState,
        CancellationToken cancellationToken)
    {
        foreach (var drinkImport in batch.ImportContent.Drinks)
        {
            var existing = await drinkRepository.GetByNameAsync(drinkImport.Name, cancellationToken);
            if (existing is null)
            {
                await CreateDrinkAsync(drinkImport, cancellationToken);
                applyState.Created++;
                continue;
            }

            if (RequiresReviewedUpdate(batch, "drink", drinkImport.Name))
            {
                await UpdateDrinkAsync(existing, drinkImport, cancellationToken);
                applyState.Updated++;
                continue;
            }

            applyState.Skipped++;
        }
    }

    private async Task CreateDrinkAsync(
        NormalizedDrinkImport drinkImport,
        CancellationToken cancellationToken)
    {
        var drink = DrinkAggregate.Create(
            DrinkNameValue.Create(drinkImport.Name),
            DrinkCategory.Create(drinkImport.Category),
            drinkImport.Description,
            drinkImport.Method,
            drinkImport.Garnish,
            DrinkImageUrl.Create(drinkImport.ImageUrl));
        drink.SetTags(await ResolveTagsAsync(drinkImport.TagNames, cancellationToken));
        drink.SetRecipeEntries(await ResolveRecipeEntriesAsync(drink.Id, drinkImport.RecipeEntries, cancellationToken));
        drinkRepository.Add(drink);
    }

    private async Task UpdateDrinkAsync(
        DrinkAggregate existing,
        NormalizedDrinkImport drinkImport,
        CancellationToken cancellationToken)
    {
        existing.Update(
            DrinkNameValue.Create(drinkImport.Name),
            DrinkCategory.Create(drinkImport.Category),
            drinkImport.Description,
            drinkImport.Method,
            drinkImport.Garnish,
            DrinkImageUrl.Create(drinkImport.ImageUrl));
        existing.SetTags(await ResolveTagsAsync(drinkImport.TagNames, cancellationToken));
        existing.SetRecipeEntries(await ResolveRecipeEntriesAsync(existing.Id, drinkImport.RecipeEntries, cancellationToken));
    }

    private static bool RequiresReviewedUpdate(ImportBatch batch, string targetType, string targetKey)
    {
        return batch.ReviewRows.Any(r =>
            r.RequiresReview &&
            string.Equals(r.TargetType, targetType, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.TargetKey, targetKey, StringComparison.OrdinalIgnoreCase));
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

    private sealed class ImportApplyState
    {
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }

        public ImportApplySummary ToSummary()
        {
            return new ImportApplySummary(Created, Updated, Skipped, 0);
        }
    }
}
