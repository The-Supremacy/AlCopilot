using System.ComponentModel;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationDrinkSearchTool(
    IMediator mediator,
    IRecommendationToolInvocationRecorder toolInvocationRecorder,
    IRecommendationExecutionTraceRecorder executionTraceRecorder)
{
    [Description("Search the drink catalog by drink name when you need to resolve an exact drink before looking up its recipe.")]
    public async Task<RecommendationDrinkSearchResult> SearchDrinksAsync(
        [Description("Drink name or partial drink name to search for.")] string query,
        [Description("Maximum number of matches to return. Keep this small and use 5 unless the user explicitly asks for more.")] int maxResults = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            executionTraceRecorder.Record(
                BuildTraceStep("invalid-input", "A drink search query is required.", null, maxResults, 0));
            return new RecommendationDrinkSearchResult(
                "invalid-input",
                "A drink search query is required.",
                []);
        }

        var normalizedQuery = query.Trim();
        var drinks = await mediator.Send(new GetRecommendationCatalogQuery(), CancellationToken.None);
        var matches = RecommendationCatalogMatcher.SearchDrinksByName(drinks, normalizedQuery, maxResults)
            .Select(RecommendationDrinkSearchItem.FromDrink)
            .ToList();

        if (matches.Count == 0)
        {
            executionTraceRecorder.Record(
                BuildTraceStep("not-found", $"No drinks were found matching '{normalizedQuery}'.", normalizedQuery, maxResults, 0));
            return new RecommendationDrinkSearchResult(
                "not-found",
                $"No drinks were found matching '{normalizedQuery}'.",
                []);
        }

        toolInvocationRecorder.Record(
            "search_drinks",
            $"Searched the catalog for drinks matching '{normalizedQuery}'.");
        executionTraceRecorder.Record(
            BuildTraceStep("ok", $"Found {matches.Count} drink(s) matching '{normalizedQuery}'.", normalizedQuery, maxResults, matches.Count));

        return new RecommendationDrinkSearchResult(
            "ok",
            $"Found {matches.Count} drink(s) matching '{normalizedQuery}'.",
            matches);
    }

    private static RecommendationExecutionTraceStep BuildTraceStep(
        string outcome,
        string summary,
        string? query,
        int maxResults,
        int returnedCount)
    {
        return new RecommendationExecutionTraceStep(
            "tool.search_drinks",
            outcome,
            summary,
            DateTimeOffset.UtcNow,
            new Dictionary<string, string?>
            {
                ["query"] = query,
                ["maxResults"] = maxResults.ToString(),
                ["returnedCount"] = returnedCount.ToString(),
            },
            []);
    }
}

internal sealed record RecommendationDrinkSearchResult(
    string Status,
    string Message,
    IReadOnlyCollection<RecommendationDrinkSearchItem> Drinks);

internal sealed record RecommendationDrinkSearchItem(
    Guid DrinkId,
    string DrinkName,
    string? Description,
    IReadOnlyCollection<string> RecipeIngredientNames)
{
    internal static RecommendationDrinkSearchItem FromDrink(AlCopilot.DrinkCatalog.Contracts.DTOs.DrinkDetailDto drink)
    {
        return new RecommendationDrinkSearchItem(
            drink.Id,
            drink.Name,
            drink.Description,
            drink.RecipeEntries
                .Select(entry => entry.Ingredient.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }
}
