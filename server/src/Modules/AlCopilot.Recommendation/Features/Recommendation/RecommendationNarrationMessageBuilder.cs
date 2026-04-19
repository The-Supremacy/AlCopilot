using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationNarrationMessageBuilder
{
    internal static RecommendationNarrationContext CreateContext(RecommendationNarrationRequest request)
    {
        var ingredientNames = BuildIngredientNameLookup(request.CatalogSnapshot);

        return new RecommendationNarrationContext(
            BuildProfileSummary(request.CustomerMessage, request.Profile, ingredientNames),
            BuildCandidateSummary(request.RecommendationGroups));
    }

    internal static IReadOnlyList<ChatMessage> BuildContextMessages(RecommendationNarrationContext context)
    {
        return
        [
            new ChatMessage(ChatRole.System, "Use the recommendation snapshot below as authoritative product context."),
            new ChatMessage(ChatRole.System, context.ProfileSummary),
            new ChatMessage(ChatRole.System, context.CandidateSummary),
        ];
    }

    private static string BuildProfileSummary(
        string customerMessage,
        CustomerProfileDto profile,
        IReadOnlyDictionary<Guid, string> ingredientNames)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Customer profile snapshot:");
        builder.AppendLine($"- favorites: {FormatIngredientList(profile.FavoriteIngredientIds, ingredientNames)}");
        builder.AppendLine($"- dislikes: {FormatIngredientList(profile.DislikedIngredientIds, ingredientNames)}");
        builder.AppendLine($"- prohibited: {FormatIngredientList(profile.ProhibitedIngredientIds, ingredientNames)}");
        builder.AppendLine($"- owned: {FormatIngredientList(profile.OwnedIngredientIds, ingredientNames)}");
        builder.AppendLine($"- current request: {customerMessage}");

        return builder.ToString().Trim();
    }

    private static string BuildCandidateSummary(IReadOnlyCollection<RecommendationGroupDto> groups)
    {
        var builder = new System.Text.StringBuilder();
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
