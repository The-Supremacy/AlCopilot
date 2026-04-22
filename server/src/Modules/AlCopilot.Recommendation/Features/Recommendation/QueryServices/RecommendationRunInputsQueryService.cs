using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationRunInputsQueryService(IMediator mediator) : IRecommendationRunInputsQueryService
{
    public async Task<RecommendationRunInputs> GetRunInputsAsync(CancellationToken cancellationToken = default)
    {
        var profile = await mediator.Send(new GetCustomerProfileQuery(), cancellationToken);
        var drinks = await mediator.Send(new GetRecommendationCatalogQuery(), cancellationToken);
        return new RecommendationRunInputs(profile, drinks);
    }
}
