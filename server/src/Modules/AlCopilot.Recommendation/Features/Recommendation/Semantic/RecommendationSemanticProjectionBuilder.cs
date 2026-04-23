using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationSemanticProjectionBuilder
{
    internal static IReadOnlyCollection<RecommendationSemanticProjectionPoint> Build(
        IReadOnlyCollection<DrinkDetailDto> drinks)
    {
        var points = new List<RecommendationSemanticProjectionPoint>();

        foreach (var drink in drinks)
        {
            points.Add(new RecommendationSemanticProjectionPoint(
                CreatePointId(drink.Id, RecommendationSemanticFacetKind.Name, drink.Name),
                drink.Id,
                drink.Name,
                drink.Category,
                RecommendationSemanticFacetKind.Name,
                drink.Name,
                null));

            if (!string.IsNullOrWhiteSpace(drink.Description))
            {
                points.Add(new RecommendationSemanticProjectionPoint(
                    CreatePointId(drink.Id, RecommendationSemanticFacetKind.Description, drink.Description),
                    drink.Id,
                    drink.Name,
                    drink.Category,
                    RecommendationSemanticFacetKind.Description,
                    drink.Description.Trim(),
                    null));
            }

            foreach (var ingredientName in drink.RecipeEntries
                         .Select(entry => entry.Ingredient.Name)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
            {
                points.Add(new RecommendationSemanticProjectionPoint(
                    CreatePointId(drink.Id, RecommendationSemanticFacetKind.Ingredient, ingredientName),
                    drink.Id,
                    drink.Name,
                    drink.Category,
                    RecommendationSemanticFacetKind.Ingredient,
                    ingredientName,
                    ingredientName));
            }
        }

        return points;
    }

    private static Guid CreatePointId(Guid drinkId, RecommendationSemanticFacetKind facetKind, string text)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{drinkId:N}|{facetKind}|{text.Trim()}"));
        var guidBytes = bytes.Take(16).ToArray();
        return new Guid(guidBytes);
    }
}
