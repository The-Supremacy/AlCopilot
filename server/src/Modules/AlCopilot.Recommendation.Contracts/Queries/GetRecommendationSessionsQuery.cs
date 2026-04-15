using AlCopilot.Recommendation.Contracts.DTOs;
using Mediator;

namespace AlCopilot.Recommendation.Contracts.Queries;

public sealed record GetRecommendationSessionsQuery() : IRequest<List<RecommendationSessionSummaryDto>>;
