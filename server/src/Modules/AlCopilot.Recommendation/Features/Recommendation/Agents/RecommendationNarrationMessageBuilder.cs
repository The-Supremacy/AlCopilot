namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal static class RecommendationRunContextMessageBuilder
{
    internal static string Build(RecommendationRunContext runContext)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Use this recommendation run context as authoritative product context for this response only.");
        builder.AppendLine("Only recommend drinks from the deterministic groups below.");
        builder.AppendLine("If exact measurements, method, garnish, or brand details are needed for a listed drink, call the lookup_drink_recipe tool.");
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

                builder.AppendLine(
                    $"  - {item.DrinkName} [id: {item.DrinkId}] (score {item.Score}; {ownedIngredients}; {missingIngredients}; {recipeIngredients}; {method}; {garnish}; {description})");
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
}
