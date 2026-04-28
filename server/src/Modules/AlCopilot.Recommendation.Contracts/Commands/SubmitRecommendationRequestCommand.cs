using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using Mediator;

namespace AlCopilot.Recommendation.Contracts.Commands;

public sealed record SubmitRecommendationRequestCommand(
    Guid? SessionId,
    string Message) : IRequest<SubmitRecommendationMessageResultDto>;

public sealed record SubmitRecommendationTurnFeedbackCommand(
    Guid SessionId,
    Guid TurnId,
    string Rating,
    string? Comment) : IRequest;

public sealed record ReplaceRecommendationSemanticCatalogCommand(
    IReadOnlyCollection<DrinkDetailDto> Drinks) : IRequest<RecommendationSemanticCatalogIndexResultDto>;
