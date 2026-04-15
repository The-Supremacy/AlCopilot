using AlCopilot.Shared.Models;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationActorResolver
{
    public static string GetCustomerId(ICurrentActorAccessor currentActorAccessor)
    {
        var actor = currentActorAccessor.GetCurrent();
        if (!actor.IsAuthenticated)
        {
            throw new InvalidOperationException("An authenticated customer identity is required.");
        }

        return actor.UserId
            ?? actor.DisplayName
            ?? throw new InvalidOperationException("An authenticated customer identity is required.");
    }
}
