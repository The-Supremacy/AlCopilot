using System.ComponentModel;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationRecipeLookupTool(
    IMediator mediator,
    IRecommendationToolInvocationRecorder toolInvocationRecorder,
    IRecommendationExecutionTraceRecorder executionTraceRecorder)
{
    [Description("Look up the full recipe details for a specific known drink from the catalog.")]
    public async Task<RecommendationRecipeLookupResult> LookupDrinkRecipeAsync(
        [Description("Optional drink id from the recommendation run context. Prefer this when available.")] string? drinkId = null,
        [Description("Optional drink name when the drink id is unavailable. Use the exact drink name if possible.")] string? drinkName = null)
    {
        var drink = await ResolveDrinkAsync(drinkId, drinkName);
        if (drink is null)
        {
            executionTraceRecorder.Record(
                new RecommendationExecutionTraceStep(
                    "tool.lookup_drink_recipe",
                    "not-found",
                    "No matching drink was found for the provided id or name.",
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, string?>
                    {
                        ["drinkId"] = drinkId,
                        ["drinkName"] = drinkName,
                    },
                    []));
            return new RecommendationRecipeLookupResult(
                "not-found",
                "No matching drink was found for the provided id or name.",
                null);
        }

        toolInvocationRecorder.Record(
            "lookup_drink_recipe",
            $"Looked up the full recipe details for {drink.Name}.");
        executionTraceRecorder.Record(
            new RecommendationExecutionTraceStep(
                "tool.lookup_drink_recipe",
                "ok",
                $"Found recipe details for {drink.Name}.",
                DateTimeOffset.UtcNow,
                new Dictionary<string, string?>
                {
                    ["drinkId"] = drink.Id.ToString(),
                    ["drinkName"] = drink.Name,
                    ["recipeEntryCount"] = drink.RecipeEntries.Count.ToString(),
                },
                []));

        return new RecommendationRecipeLookupResult(
            "ok",
            $"Found recipe details for {drink.Name}.",
            RecommendationRecipeLookupDrink.FromDrink(drink));
    }

    private async Task<DrinkDetailDto?> ResolveDrinkAsync(string? drinkId, string? drinkName)
    {
        if (Guid.TryParse(drinkId, out var parsedDrinkId))
        {
            var drinkById = await mediator.Send(new GetDrinkByIdQuery(parsedDrinkId), CancellationToken.None);
            if (drinkById is not null)
            {
                return drinkById;
            }
        }

        if (string.IsNullOrWhiteSpace(drinkName))
        {
            return null;
        }

        var drinks = await mediator.Send(new GetRecommendationCatalogQuery(), CancellationToken.None);
        return RecommendationCatalogMatcher.FindDrink(drinks, drinkId, drinkName);
    }
}

internal sealed record RecommendationRecipeLookupResult(
    string Status,
    string Message,
    RecommendationRecipeLookupDrink? Drink);

internal sealed record RecommendationRecipeLookupDrink(
    Guid DrinkId,
    string DrinkName,
    string? Description,
    string? Method,
    string? Garnish,
    IReadOnlyCollection<RecommendationRecipeLookupEntry> RecipeEntries)
{
    internal static RecommendationRecipeLookupDrink FromDrink(DrinkDetailDto drink)
    {
        return new RecommendationRecipeLookupDrink(
            drink.Id,
            drink.Name,
            drink.Description,
            drink.Method,
            drink.Garnish,
            drink.RecipeEntries
                .Select(entry => new RecommendationRecipeLookupEntry(
                    entry.Ingredient.Name,
                    entry.Quantity,
                    entry.RecommendedBrand,
                    entry.Ingredient.NotableBrands))
                .ToList());
    }
}

internal sealed record RecommendationRecipeLookupEntry(
    string IngredientName,
    string Quantity,
    string? RecommendedBrand,
    IReadOnlyCollection<string> NotableBrands);
