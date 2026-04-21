using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationNarrationContextProvider(IServiceScopeFactory scopeFactory) : MessageAIContextProvider
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

        await using var scope = scopeFactory.CreateAsyncScope();
        var queryService = scope.ServiceProvider.GetRequiredService<IRecommendationNarrationContextQueryService>();
        var snapshot = await queryService.GetSnapshotAsync(customerMessage, cancellationToken);

        return
        [
            new ChatMessage(
                ChatRole.System,
                RecommendationNarrationMessageBuilder.BuildCurrentRecommendationSnapshot(snapshot)),
        ];
    }
}
