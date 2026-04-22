using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class NoOpChatHistoryProvider : ChatHistoryProvider
{
    protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IEnumerable<ChatMessage>>([]);
    }

    protected override ValueTask StoreChatHistoryAsync(
        InvokedContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }
}
