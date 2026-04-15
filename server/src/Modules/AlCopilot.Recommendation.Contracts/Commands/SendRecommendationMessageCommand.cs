using AlCopilot.Recommendation.Contracts.DTOs;
using Mediator;

namespace AlCopilot.Recommendation.Contracts.Commands;

public sealed record SubmitRecommendationRequestCommand(
    Guid? SessionId,
    string Message) : IRequest<RecommendationSessionDto>;
