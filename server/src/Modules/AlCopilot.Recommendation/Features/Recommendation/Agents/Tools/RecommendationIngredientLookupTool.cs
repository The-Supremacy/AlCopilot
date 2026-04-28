using System.ComponentModel;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationIngredientLookupTool(
    IMediator mediator,
    IRecommendationCatalogFuzzyLookupService fuzzyLookupService,
    IRecommendationExecutionTraceRecorder executionTraceRecorder)
{
    [Description("Find catalog drinks that contain a requested ingredient. Use for 'which drinks use X', 'drinks with X', or ingredient-led recommendation requests.")]
    public async Task<RecommendationIngredientLookupResult> LookupDrinksByIngredientAsync(
        [Description("Ingredient name to search for, such as Ginger Beer, Tequila, Lime Juice, Rum, or Gin. Do not pass drink names here.")] string ingredientName,
        [Description("Maximum number of matching drinks to return. Use 5 unless the user asks for a different count.")] int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
        {
            executionTraceRecorder.Record(
                BuildTraceStep("invalid-input", "An ingredient name is required.", null, maxResults, 0));
            return new RecommendationIngredientLookupResult(
                "invalid-input",
                "An ingredient name is required.",
                []);
        }

        var normalizedIngredientName = ingredientName.Trim();
        var drinks = await mediator.Send(new GetRecommendationCatalogQuery(), cancellationToken);
        var resolvedIngredientName = await ResolveIngredientNameAsync(normalizedIngredientName, cancellationToken);
        var matches = RecommendationCatalogMatcher.FindDrinksByIngredient(
                drinks,
                resolvedIngredientName,
                Math.Clamp(maxResults, 1, 10))
            .Select(drink => RecommendationIngredientLookupDrink.FromDrink(
                drink,
                RecommendationCatalogMatcher.GetMatchedIngredientNames(drink, resolvedIngredientName)))
            .ToList();

        if (matches.Count == 0)
        {
            executionTraceRecorder.Record(
                BuildTraceStep("not-found", $"No drinks were found containing an ingredient matching '{normalizedIngredientName}'.", normalizedIngredientName, maxResults, 0));
            return new RecommendationIngredientLookupResult(
                "not-found",
                $"No drinks were found containing an ingredient matching '{normalizedIngredientName}'.",
                []);
        }

        executionTraceRecorder.Record(
            BuildTraceStep("ok", $"Found {matches.Count} drink(s) containing an ingredient matching '{normalizedIngredientName}'.", normalizedIngredientName, maxResults, matches.Count));

        return new RecommendationIngredientLookupResult(
            "ok",
            $"Found {matches.Count} drink(s) containing an ingredient matching '{normalizedIngredientName}'.",
            matches);
    }

    private async Task<string> ResolveIngredientNameAsync(
        string normalizedIngredientName,
        CancellationToken cancellationToken)
    {
        var fuzzyMatches = await fuzzyLookupService.FindIngredientMatchesAsync(
            normalizedIngredientName,
            cancellationToken);

        return fuzzyMatches
            .OrderByDescending(match => match.Similarity)
            .Select(match => match.Name)
            .FirstOrDefault() ?? normalizedIngredientName;
    }

    private static RecommendationExecutionTraceStep BuildTraceStep(
        string outcome,
        string summary,
        string? ingredientName,
        int maxResults,
        int returnedCount)
    {
        return new RecommendationExecutionTraceStep(
            "tool.lookup_drinks_by_ingredient",
            outcome,
            summary,
            DateTimeOffset.UtcNow,
            new Dictionary<string, string?>
            {
                ["ingredientName"] = ingredientName,
                ["maxResults"] = maxResults.ToString(),
                ["returnedCount"] = returnedCount.ToString(),
            },
            []);
    }
}

internal sealed record RecommendationIngredientLookupResult(
    string Status,
    string Message,
    IReadOnlyCollection<RecommendationIngredientLookupDrink> Drinks);

internal sealed record RecommendationIngredientLookupDrink(
    Guid DrinkId,
    string DrinkName,
    string? Description,
    IReadOnlyCollection<string> MatchedIngredientNames,
    IReadOnlyCollection<string> RecipeIngredientNames,
    string? Method,
    string? Garnish)
{
    internal static RecommendationIngredientLookupDrink FromDrink(
        DrinkDetailDto drink,
        IReadOnlyCollection<string> matchedIngredientNames)
    {
        return new RecommendationIngredientLookupDrink(
            drink.Id,
            drink.Name,
            drink.Description,
            matchedIngredientNames,
            drink.RecipeEntries
                .Select(entry => entry.Ingredient.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            drink.Method,
            drink.Garnish);
    }
}
