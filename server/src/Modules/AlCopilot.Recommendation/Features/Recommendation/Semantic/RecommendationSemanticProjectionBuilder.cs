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
            if (string.IsNullOrWhiteSpace(drink.Description))
            {
                continue;
            }

            points.Add(new RecommendationSemanticProjectionPoint(
                CreatePointId(drink.Id, RecommendationSemanticFacetKind.Description, drink.Description),
                drink.Id,
                drink.Name,
                drink.Category,
                RecommendationSemanticFacetKind.Description,
                drink.Description.Trim(),
                null));
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
