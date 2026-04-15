using AlCopilot.Recommendation.Contracts.DTOs;
using Mediator;

namespace AlCopilot.Recommendation.Contracts.Queries;

public sealed record GetRecommendationSessionQuery(Guid SessionId) : IRequest<RecommendationSessionDto?>;
