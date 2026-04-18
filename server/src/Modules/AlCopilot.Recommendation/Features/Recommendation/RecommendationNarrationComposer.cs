using System.Text;
using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationNarrationComposer : IRecommendationNarrationComposer
{
    public string BuildContextInstructions(
        string customerMessage,
        CustomerProfileDto profile,
        IReadOnlyCollection<RecommendationGroupDto> recommendationGroups,
        IReadOnlyCollection<DrinkDetailDto> catalogSnapshot)
    {
        return BuildContextMessage(customerMessage, profile, recommendationGroups, catalogSnapshot);
    }

    private static string BuildContextMessage(
        string customerMessage,
        CustomerProfileDto profile,
        IReadOnlyCollection<RecommendationGroupDto> groups,
        IReadOnlyCollection<DrinkDetailDto> catalogSnapshot)
    {
        var ingredientNames = BuildIngredientNameLookup(catalogSnapshot);
        var builder = new StringBuilder();
        builder.AppendLine("Customer profile context:");
        builder.AppendLine($"- favorites: {FormatIngredientList(profile.FavoriteIngredientIds, ingredientNames)}");
        builder.AppendLine($"- dislikes: {FormatIngredientList(profile.DislikedIngredientIds, ingredientNames)}");
        builder.AppendLine($"- prohibited: {FormatIngredientList(profile.ProhibitedIngredientIds, ingredientNames)}");
        builder.AppendLine($"- owned: {FormatIngredientList(profile.OwnedIngredientIds, ingredientNames)}");
        builder.AppendLine($"- current request: {customerMessage}");
        builder.AppendLine();
        builder.AppendLine("Deterministic candidate groups:");

        foreach (var group in groups.Where(group => group.Items.Count > 0))
        {
            builder.AppendLine($"- {group.Label}:");

            foreach (var item in group.Items.Take(5))
            {
                var description = string.IsNullOrWhiteSpace(item.Description)
                    ? "no additional description"
                    : item.Description;
                var missingIngredients = item.MissingIngredientNames.Count == 0
                    ? "available now"
                    : $"missing {string.Join(", ", item.MissingIngredientNames)}";
                var matchedSignals = item.MatchedSignals.Count == 0
                    ? "no matched signals"
                    : $"signals: {string.Join(", ", item.MatchedSignals)}";

                builder.AppendLine(
                    $"  - {item.DrinkName} (score {item.Score}; {missingIngredients}; {matchedSignals}; {description})");
            }
        }

        return builder.ToString().Trim();
    }

    private static Dictionary<Guid, string> BuildIngredientNameLookup(IReadOnlyCollection<DrinkDetailDto> catalogSnapshot)
    {
        return catalogSnapshot
            .SelectMany(drink => drink.RecipeEntries)
            .Select(entry => entry.Ingredient)
            .GroupBy(ingredient => ingredient.Id)
            .ToDictionary(
                group => group.Key,
                group => group.Select(ingredient => ingredient.Name)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .First());
    }

    private static string FormatIngredientList(
        IReadOnlyCollection<Guid> ids,
        IReadOnlyDictionary<Guid, string> ingredientNames)
    {
        if (ids.Count == 0)
        {
            return "none";
        }

        return string.Join(
            ", ",
            ids.Select(id => ingredientNames.TryGetValue(id, out var name) ? name : id.ToString())
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
    }
}
