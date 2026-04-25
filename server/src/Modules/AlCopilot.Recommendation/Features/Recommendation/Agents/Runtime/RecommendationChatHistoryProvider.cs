using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationChatHistoryProvider(
    ChatSession session,
    int maxHistoryMessages,
    RecommendationAgentTurnState turnState) : ChatHistoryProvider
{
    protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var messages = BuildChatMessages(session.Turns, maxHistoryMessages);

        return ValueTask.FromResult<IEnumerable<ChatMessage>>(messages);
    }

    internal static List<ChatMessage> BuildChatMessages(
        IEnumerable<ChatTurn> turns,
        int maxHistoryMessages)
    {
        var orderedTurns = turns
            .OrderBy(turn => turn.Sequence)
            .ToList();
        var includedTurns = maxHistoryMessages > 0
            ? orderedTurns.TakeLast(maxHistoryMessages)
            : orderedTurns;
        return includedTurns
            .Select(turn => new ChatMessage(
                string.Equals(turn.Role, "assistant", StringComparison.Ordinal) ? ChatRole.Assistant : ChatRole.User,
                turn.Content))
            .ToList();
    }

    protected override ValueTask StoreChatHistoryAsync(
        InvokedContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestMessages = context.RequestMessages ?? [];
        var responseMessages = context.ResponseMessages ?? [];
        var userMessage = requestMessages
            .Where(message => message.Role == ChatRole.User)
            .Select(message => message.Text)
            .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));
        var assistantMessage = responseMessages
            .Where(message => message.Role == ChatRole.Assistant)
            .Select(message => message.Text)
            .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));
        if (string.IsNullOrWhiteSpace(userMessage) || string.IsNullOrWhiteSpace(assistantMessage))
        {
            return ValueTask.CompletedTask;
        }

        turnState.CustomerMessage ??= userMessage;
        session.AppendUserTurn(userMessage);
        session.AppendAssistantTurn(assistantMessage, [], []);

        return ValueTask.CompletedTask;
    }
}
