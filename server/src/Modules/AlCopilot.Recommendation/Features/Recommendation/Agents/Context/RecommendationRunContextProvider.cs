using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationRunContextProvider(
    IRecommendationCurrentRunContextAccessor currentRunContextAccessor,
    IRecommendationRunContextFactory runContextFactory) : MessageAIContextProvider
{
    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var customerMessage = context.RequestMessages
            .Where(message => message.Role == ChatRole.User)
            .Select(message => message.Text)
            .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));
        if (string.IsNullOrWhiteSpace(customerMessage))
        {
            return [];
        }

        var runContext = currentRunContextAccessor.Current
            ?? await runContextFactory.CreateAsync(customerMessage, cancellationToken);
        currentRunContextAccessor.Current = runContext;

        return
        [
            new ChatMessage(
                ChatRole.System,
                RecommendationRunContextMessageBuilder.Build(runContext)),
        ];
    }
}
