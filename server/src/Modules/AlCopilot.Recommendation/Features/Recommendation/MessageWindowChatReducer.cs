using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class MessageWindowChatReducer(int maxNonSystemMessages) : IChatReducer
{
    private readonly int _maxNonSystemMessages = Math.Max(1, maxNonSystemMessages);

    public Task<IEnumerable<ChatMessage>> ReduceAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var materialized = messages.ToList();
        var systemMessages = materialized
            .Where(message => message.Role == ChatRole.System)
            .ToList();
        var nonSystemMessages = materialized
            .Where(message => message.Role != ChatRole.System)
            .TakeLast(_maxNonSystemMessages)
            .ToList();

        return Task.FromResult<IEnumerable<ChatMessage>>([.. systemMessages, .. nonSystemMessages]);
    }
}
