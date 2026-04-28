using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationRequestIntentResolver(
    IRecommendationCatalogFuzzyLookupService fuzzyLookupService) : IRecommendationRequestIntentResolver
{
    public async Task<RecommendationRequestIntent> ResolveAsync(
        string customerMessage,
        RecommendationRunInputs inputs,
        RecommendationSemanticSearchResult semanticSearchResult,
        CancellationToken cancellationToken = default)
    {
        var analysis = RecommendationRequestQueryAnalyzer.Analyze(customerMessage);
        var entities = await ResolveEntitiesAsync(analysis, inputs, cancellationToken);
        var kind = ResolveKind(
            entities.RequestedDrinkName,
            analysis.LooksLikeDrinkDetails);

        return new RecommendationRequestIntent(
            kind,
            entities.RequestedDrinkName,
            entities.RequestedIngredientNames,
            analysis.RequestDescriptors,
            analysis.LooksLikeDrinkDetails,
            entities.ExcludedIngredientNames);
    }

    private async Task<RecommendationResolvedRequestEntities> ResolveEntitiesAsync(
        RecommendationRequestQueryAnalysis analysis,
        RecommendationRunInputs inputs,
        CancellationToken cancellationToken)
    {
        var requestedDrinkName = RecommendationCatalogMatcher.FindMentionedDrinkName(
            inputs.Drinks,
            analysis.NormalizedMessage);
        var mentionedIngredientNames = RecommendationCatalogMatcher.FindMentionedIngredientNames(
            inputs.Drinks,
            analysis.NormalizedMessage)
            .ToList();
        var excludedIngredientNames = mentionedIngredientNames
            .Where(ingredientName => IsExcludedIngredientMention(analysis.NormalizedMessage, ingredientName))
            .ToList();
        var requestedIngredientNames = mentionedIngredientNames
            .Except(excludedIngredientNames, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (string.IsNullOrWhiteSpace(requestedDrinkName) && !string.IsNullOrWhiteSpace(analysis.DrinkSearchText))
        {
            var fuzzyDrinkMatches = await fuzzyLookupService.FindDrinkMatchesAsync(
                analysis.DrinkSearchText,
                cancellationToken);
            requestedDrinkName = ResolveClearWinner(fuzzyDrinkMatches);
        }

        // IngredientSearchTexts are raw ingredient-like phrases extracted from the prompt.
        // requestedIngredientNames contains only exact catalog ingredient matches.
        // If we extracted phrases but still have no exact ingredient matches, try fuzzy lookup next.
        if (requestedIngredientNames.Count == 0 && analysis.IngredientSearchTexts.Count > 0)
        {
            foreach (var ingredientSearchText in analysis.IngredientSearchTexts)
            {
                var fuzzyIngredientMatches = await fuzzyLookupService.FindIngredientMatchesAsync(
                    ingredientSearchText,
                    cancellationToken);
                var requestedIngredientName = ResolveClearWinner(fuzzyIngredientMatches);
                if (!string.IsNullOrWhiteSpace(requestedIngredientName))
                {
                    requestedIngredientNames.Add(requestedIngredientName);
                }
            }
        }

        return new RecommendationResolvedRequestEntities(
            requestedDrinkName,
            requestedIngredientNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            excludedIngredientNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }

    private static bool IsExcludedIngredientMention(string customerMessage, string ingredientName)
    {
        var lowered = customerMessage.ToLowerInvariant();
        var loweredIngredientName = ingredientName.ToLowerInvariant();
        var exclusionSignals = new[]
        {
            $"no {loweredIngredientName}",
            $"without {loweredIngredientName}",
            $"skip {loweredIngredientName}",
            $"skipping {loweredIngredientName}",
            $"avoid {loweredIngredientName}",
            $"avoiding {loweredIngredientName}",
        };

        return exclusionSignals.Any(signal => lowered.Contains(signal, StringComparison.Ordinal));
    }

    private static string? ResolveClearWinner(IReadOnlyCollection<RecommendationFuzzyMatch> matches)
    {
        var ordered = matches
            .OrderByDescending(match => match.Similarity)
            .ToList();
        if (ordered.Count == 0)
        {
            return null;
        }

        var best = ordered[0];
        var runnerUpSimilarity = ordered.Count > 1 ? ordered[1].Similarity : 0d;

        return best.Similarity >= 0.55d && best.Similarity - runnerUpSimilarity >= 0.08d
            ? best.Name
            : null;
    }

    private static RecommendationRequestIntentKind ResolveKind(
        string? requestedDrinkName,
        bool looksLikeDrinkDetails)
    {
        if (looksLikeDrinkDetails || !string.IsNullOrWhiteSpace(requestedDrinkName))
        {
            return RecommendationRequestIntentKind.DrinkDetails;
        }

        return RecommendationRequestIntentKind.Recommendation;
    }
    private sealed record RecommendationResolvedRequestEntities(
        string? RequestedDrinkName,
        IReadOnlyCollection<string> RequestedIngredientNames,
        IReadOnlyCollection<string> ExcludedIngredientNames);
}
