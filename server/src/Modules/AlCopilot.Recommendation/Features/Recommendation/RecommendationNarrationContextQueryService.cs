using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationNarrationContextQueryService(
    IMediator mediator,
    IRecommendationCandidateBuilder candidateBuilder) : IRecommendationNarrationContextQueryService
{
    public async Task<RecommendationNarrationSnapshot> GetSnapshotAsync(
        string customerMessage,
        CancellationToken cancellationToken = default)
    {
        var profile = await mediator.Send(new GetCustomerProfileQuery(), cancellationToken);
        var drinks = await mediator.Send(new GetRecommendationCatalogQuery(), cancellationToken);
        var groups = candidateBuilder.Build(customerMessage, profile, drinks);

        return new RecommendationNarrationSnapshot(profile, groups, drinks);
    }
}
