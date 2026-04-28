namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal static class RecommendationRunContextMessageBuilder
{
    internal static string Build(RecommendationRunContext runContext)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Recommendation run context for this response only:");
        builder.AppendLine("Resolved request intent:");
        builder.AppendLine($"- kind: {runContext.Intent.Kind}");
        builder.AppendLine($"- requested drink: {runContext.Intent.RequestedDrinkName ?? "none"}");
        builder.AppendLine($"- requested ingredients: {FormatTextList(runContext.Intent.RequestedIngredientNames)}");
        builder.AppendLine($"- excluded ingredients: {FormatTextList(runContext.Intent.CurrentExcludedIngredientNames)}");
        builder.AppendLine($"- request descriptors: {FormatTextList(runContext.Intent.RequestDescriptors)}");
        builder.AppendLine($"- semantic hints: {FormatTextList(runContext.SemanticSummaryHints)}");
        builder.AppendLine();
        builder.AppendLine("Customer profile:");
        builder.AppendLine($"- favorites: {FormatIngredientList(runContext.Profile.FavoriteIngredientIds, runContext)}");
        builder.AppendLine($"- dislikes: {FormatIngredientList(runContext.Profile.DislikedIngredientIds, runContext)}");
        builder.AppendLine($"- prohibited: {FormatIngredientList(runContext.Profile.ProhibitedIngredientIds, runContext)}");
        builder.AppendLine($"- owned: {FormatIngredientList(runContext.Profile.OwnedIngredientIds, runContext)}");
        builder.AppendLine();
        builder.AppendLine("Grounded candidate groups, ordered by deterministic fit:");

        foreach (var group in runContext.Groups.Where(group => group.Items.Count > 0))
        {
            builder.AppendLine($"- {group.Label}:");

            foreach (var item in group.Items.Take(5))
            {
                var description = string.IsNullOrWhiteSpace(item.Description)
                    ? "no additional description"
                    : item.Description;
                var ownedIngredients = item.OwnedIngredientNames.Count == 0
                    ? "owned none"
                    : $"owned {string.Join(", ", item.OwnedIngredientNames)}";
                var missingIngredients = item.MissingIngredientNames.Count == 0
                    ? "missing none"
                    : $"missing {string.Join(", ", item.MissingIngredientNames)}";
                var dislikedIngredients = item.DislikedIngredientNames.Count == 0
                    ? "disliked none"
                    : $"disliked {string.Join(", ", item.DislikedIngredientNames)}";
                var matchedSignals = item.MatchedSignals.Count == 0
                    ? "matched signals none"
                    : $"matched signals {string.Join(", ", item.MatchedSignals)}";
                var semanticHints = item.SemanticHints.Count == 0
                    ? "semantic hints none"
                    : $"semantic hints {string.Join(", ", item.SemanticHints)}";
                var recipeContext = BuildRecipeContext(runContext, item);

                builder.AppendLine(
                    $"  - {item.DrinkName} [id: {item.DrinkId}] ({ownedIngredients}; {missingIngredients}; {dislikedIngredients}; {recipeContext}{matchedSignals}; {semanticHints}; {description})");
            }
        }

        return builder.ToString().Trim();
    }

    private static string FormatIngredientList(
        IReadOnlyCollection<Guid> ids,
        RecommendationRunContext runContext)
    {
        if (ids.Count == 0)
        {
            return "none";
        }

        var ingredientNames = BuildIngredientNameLookup(runContext);
        return string.Join(
            ", ",
            ids.Select(id => ingredientNames.TryGetValue(id, out var name) ? name : id.ToString())
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
    }

    private static IReadOnlyDictionary<Guid, string> BuildIngredientNameLookup(RecommendationRunContext runContext)
    {
        return runContext.IngredientNames;
    }

    private static string BuildRecipeContext(
        RecommendationRunContext runContext,
        RecommendationRunContextItem item)
    {
        if (runContext.Intent.IsDrinkDetailsRequest)
        {
            return string.Empty;
        }

        var recipeIngredients = item.RecipeIngredientNames.Count == 0
            ? "recipe ingredients unavailable"
            : $"recipe {string.Join(", ", item.RecipeIngredientNames)}";
        var method = string.IsNullOrWhiteSpace(item.Method)
            ? "method not specified"
            : $"method {item.Method}";
        var garnish = string.IsNullOrWhiteSpace(item.Garnish)
            ? "garnish not specified"
            : $"garnish {item.Garnish}";

        return $"{recipeIngredients}; {method}; {garnish}; ";
    }

    private static string FormatTextList(IReadOnlyCollection<string> values)
    {
        return values.Count == 0
            ? "none"
            : string.Join(", ", values.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
    }
}
