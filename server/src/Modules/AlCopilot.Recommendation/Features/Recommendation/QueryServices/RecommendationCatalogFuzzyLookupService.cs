using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Shared.Text;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationCatalogFuzzyLookupService(IMediator mediator) : IRecommendationCatalogFuzzyLookupService
{
    public async Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindDrinkMatchesAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        var normalized = searchText.NullIfWhiteSpace();
        if (normalized is null)
        {
            return [];
        }

        var matches = await mediator.Send(new FindFuzzyDrinkMatchesQuery(normalized), cancellationToken);
        return matches
            .Select(match => new RecommendationFuzzyMatch(match.DrinkId, match.DrinkName, match.Similarity))
            .ToList();
    }

    public async Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindIngredientMatchesAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        var normalized = searchText.NullIfWhiteSpace();
        if (normalized is null)
        {
            return [];
        }

        var matches = await mediator.Send(new FindFuzzyIngredientMatchesQuery(normalized), cancellationToken);
        return matches
            .Select(match => new RecommendationFuzzyMatch(match.IngredientId, match.IngredientName, match.Similarity))
            .ToList();
    }
}
