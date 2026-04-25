using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationInvocationContextProvider(
    Guid recommendationSessionId,
    RecommendationAgentTurnState turnState) : AIContextProvider
{
    protected override ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var customerMessage = context.AIContext.Messages?
            .Where(message => message.Role == ChatRole.User)
            .Select(message => message.Text)
            .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));

        turnState.RecommendationSessionId = recommendationSessionId;
        turnState.CustomerMessage = customerMessage;
        turnState.RequestAnalysis = string.IsNullOrWhiteSpace(customerMessage)
            ? null
            : RecommendationRequestQueryAnalyzer.Analyze(customerMessage);

        return ValueTask.FromResult(new AIContext());
    }
}
