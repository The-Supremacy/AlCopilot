using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationChatHistoryProvider(
    Data.RecommendationDbContext dbContext,
    ChatSession session,
    Guid agentRunId) : ChatHistoryProvider
{
    private static readonly JsonSerializerOptions NativeMessageJsonOptions = new(AIJsonUtilities.DefaultOptions)
    {
        AllowOutOfOrderMetadataProperties = true,
    };

    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var persistedMessages = await dbContext.AgentMessages
            .AsNoTracking()
            .Where(message => message.ChatSessionId == session.Id)
            .OrderBy(message => message.Sequence)
            .ToListAsync(cancellationToken);

        return BuildChatMessages(persistedMessages);
    }

    internal static List<ChatMessage> BuildChatMessages(IEnumerable<AgentMessage> messages) =>
        messages
            .OrderBy(message => message.Sequence)
            .Select(ToChatMessage)
            .ToList();

    protected override async ValueTask StoreChatHistoryAsync(
        InvokedContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingMessages = await dbContext.AgentMessages
            .Where(message => message.ChatSessionId == session.Id)
            .Select(message => new { message.NativeMessageId, message.Sequence })
            .ToListAsync(cancellationToken);
        var existingNativeMessageIds = existingMessages
            .Select(message => message.NativeMessageId)
            .ToHashSet(StringComparer.Ordinal);
        var nextSequence = existingMessages
            .Select(message => message.Sequence)
            .DefaultIfEmpty(0)
            .Max() + 1;

        foreach (var message in EnumerateMessages(context))
        {
            var nativeMessageId = EnsureNativeMessageId(message);
            if (!existingNativeMessageIds.Add(nativeMessageId))
            {
                continue;
            }

            var agentMessage = AgentMessage.Create(
                session.Id,
                agentRunId,
                nextSequence,
                nativeMessageId,
                ToRole(message.Role),
                DetermineKind(message),
                "maf",
                message.Text,
                SerializeMessage(message));

            dbContext.AgentMessages.Add(agentMessage);
            nextSequence++;
        }
    }

    private static IEnumerable<ChatMessage> EnumerateMessages(InvokedContext context)
    {
        foreach (var message in context.RequestMessages ?? [])
        {
            yield return message;
        }

        foreach (var message in context.ResponseMessages ?? [])
        {
            yield return message;
        }
    }

    private static ChatMessage ToChatMessage(AgentMessage message)
    {
        var chatMessage = JsonSerializer.Deserialize<ChatMessage>(
            message.RawMessageJson,
            NativeMessageJsonOptions) ?? new ChatMessage(ToChatRole(message.Role), message.TextContent);

        chatMessage.MessageId = message.NativeMessageId;
        return chatMessage;
    }

    private static string EnsureNativeMessageId(ChatMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.MessageId))
        {
            return message.MessageId;
        }

        message.MessageId = Guid.NewGuid().ToString("N");
        return message.MessageId;
    }

    private static string SerializeMessage(ChatMessage message) =>
        JsonSerializer.Serialize(message, NativeMessageJsonOptions);

    private static string ToRole(ChatRole role) =>
        role == ChatRole.Assistant ? "assistant"
        : role == ChatRole.System ? "system"
        : role == ChatRole.Tool ? "tool"
        : "user";

    private static ChatRole ToChatRole(string role) =>
        role switch
        {
            "assistant" => ChatRole.Assistant,
            "system" => ChatRole.System,
            "tool" => ChatRole.Tool,
            _ => ChatRole.User,
        };

    private static string DetermineKind(ChatMessage message) =>
        message.Contents
            .Select(content => content switch
            {
                FunctionCallContent => "tool-call",
                ToolCallContent => "tool-call",
                FunctionResultContent => "tool-result",
                ToolResultContent => "tool-result",
                TextReasoningContent => string.IsNullOrWhiteSpace(message.Text) ? "reasoning" : "text",
                _ => null,
            })
            .FirstOrDefault(kind => kind is not null)
        ?? (string.IsNullOrWhiteSpace(message.Text) ? "native" : "text");
}
