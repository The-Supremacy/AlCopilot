using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationChatHistoryProvider(
    ChatSession session,
    int maxHistoryTurns) : ChatHistoryProvider
{
    protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        var messages = session.Turns
            .OrderBy(turn => turn.Sequence)
            .TakeLast(Math.Max(1, maxHistoryTurns))
            .Select(MapTurnToMessage)
            .ToList();

        return new ValueTask<IEnumerable<ChatMessage>>(messages);
    }

    private static ChatMessage MapTurnToMessage(ChatTurn turn)
    {
        return turn.Role switch
        {
            "assistant" => new ChatMessage(ChatRole.Assistant, turn.Content),
            "user" => new ChatMessage(ChatRole.User, turn.Content),
            _ => throw new InvalidOperationException(
                $"Recommendation chat turn role '{turn.Role}' is not supported for Agent Framework history mapping."),
        };
    }
}
