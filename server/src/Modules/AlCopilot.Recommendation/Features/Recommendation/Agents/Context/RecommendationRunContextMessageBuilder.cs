namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal static class RecommendationRunContextMessageBuilder
{
    internal static string Build(RecommendationRunContext runContext)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Use this recommendation run context as authoritative product context for this response only.");
        builder.AppendLine("Use chat history to resolve follow-up references like that, it, or the first one.");
        builder.AppendLine("Follow the resolved request intent below before choosing tools or writing the answer.");
        builder.AppendLine("Prefer drinks from the deterministic groups when they satisfy the request.");
        builder.AppendLine("If the request asks for a prohibited ingredient, explain the conflict and do not recommend drinks containing it.");
        builder.AppendLine("Prefer drinks without disliked ingredients when a suitable option exists.");
        builder.AppendLine("If mentioning a drink with a disliked ingredient, make the tradeoff explicit instead of presenting it as an equal recommendation.");
        builder.AppendLine("If you need to resolve a drink name, call the search_drinks tool first.");
        builder.AppendLine("If the request includes ingredient constraints or the deterministic groups are not enough, call the lookup_drinks_by_ingredient tool.");
        builder.AppendLine("If deterministic candidates already include enough ingredients and method detail, do not call lookup_drink_recipe just to summarize a recommendation.");
        builder.AppendLine("For drink-details requests about how to make a specific drink, call the lookup_drink_recipe tool before answering.");
        builder.AppendLine("If exact measurements, method, garnish, or brand details are needed for a specific drink, call the lookup_drink_recipe tool.");
        builder.AppendLine();
        builder.AppendLine("Resolved request intent:");
        builder.AppendLine($"- kind: {runContext.Intent.Kind}");
        builder.AppendLine($"- requested drink: {runContext.Intent.RequestedDrinkName ?? "none"}");
        builder.AppendLine($"- requested ingredients: {FormatTextList(runContext.Intent.RequestedIngredientNames)}");
        builder.AppendLine($"- request descriptors: {FormatTextList(runContext.Intent.RequestDescriptors)}");
        builder.AppendLine($"- semantic hints: {FormatTextList(runContext.SemanticSummaryHints)}");
        builder.AppendLine();
        builder.AppendLine("Customer profile:");
        builder.AppendLine($"- favorites: {FormatIngredientList(runContext.Profile.FavoriteIngredientIds, runContext)}");
        builder.AppendLine($"- dislikes: {FormatIngredientList(runContext.Profile.DislikedIngredientIds, runContext)}");
        builder.AppendLine($"- prohibited: {FormatIngredientList(runContext.Profile.ProhibitedIngredientIds, runContext)}");
        builder.AppendLine($"- owned: {FormatIngredientList(runContext.Profile.OwnedIngredientIds, runContext)}");
        builder.AppendLine();
        builder.AppendLine("Deterministic candidate groups:");

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
                var recipeIngredients = item.RecipeIngredientNames.Count == 0
                    ? "recipe ingredients unavailable"
                    : $"recipe {string.Join(", ", item.RecipeIngredientNames)}";
                var method = string.IsNullOrWhiteSpace(item.Method)
                    ? "method not specified"
                    : $"method {item.Method}";
                var garnish = string.IsNullOrWhiteSpace(item.Garnish)
                    ? "garnish not specified"
                    : $"garnish {item.Garnish}";
                var matchedSignals = item.MatchedSignals.Count == 0
                    ? "matched signals none"
                    : $"matched signals {string.Join(", ", item.MatchedSignals)}";
                var semanticHints = item.SemanticHints.Count == 0
                    ? "semantic hints none"
                    : $"semantic hints {string.Join(", ", item.SemanticHints)}";

                builder.AppendLine(
                    $"  - {item.DrinkName} [id: {item.DrinkId}] (score {item.Score}; {ownedIngredients}; {missingIngredients}; {recipeIngredients}; {method}; {garnish}; {matchedSignals}; {semanticHints}; {description})");
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

    private static string FormatTextList(IReadOnlyCollection<string> values)
    {
        return values.Count == 0
            ? "none"
            : string.Join(", ", values.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
    }
}
