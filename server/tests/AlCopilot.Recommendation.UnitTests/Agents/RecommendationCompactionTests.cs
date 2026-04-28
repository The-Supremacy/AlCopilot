using Microsoft.Agents.AI.Compaction;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Agents;

public sealed class RecommendationCompactionTests
{
    [Fact]
    public async Task ToolResultCompaction_CollapsesOlderToolGroups_AndPreservesRecentGroup()
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are an experienced bartender."),
        };

        for (var index = 1; index <= 4; index++)
        {
            messages.Add(new ChatMessage(ChatRole.User, $"How do I make drink {index}?"));
            messages.Add(new ChatMessage(
                ChatRole.Assistant,
                [
                    new FunctionCallContent(
                        $"recipe-call-{index}",
                        "lookup_drink_recipe",
                        new Dictionary<string, object?>
                        {
                            ["drinkName"] = $"Drink {index}",
                        }),
                ]));
            messages.Add(new ChatMessage(
                ChatRole.Tool,
                [
                    new FunctionResultContent(
                        $"recipe-call-{index}",
                        $"Recipe result {index} with detailed method and measurements."),
                ]));
        }

#pragma warning disable MAAI001
        var strategy = new ToolResultCompactionStrategy(
            CompactionTriggers.GroupsExceed(3),
            minimumPreservedGroups: 1);
        var compacted = (await CompactionProvider.CompactAsync(
                strategy,
                messages,
                NullLogger.Instance,
                CancellationToken.None))
            .ToList();
#pragma warning restore MAAI001

        compacted.Count.ShouldBeLessThan(messages.Count);
        compacted.Where(message => message.Role == ChatRole.User)
            .Select(message => message.Text)
            .ShouldBe(
            [
                "How do I make drink 1?",
                "How do I make drink 2?",
                "How do I make drink 3?",
                "How do I make drink 4?",
            ]);

        var compactedSummaries = compacted
            .Where(message => message.Role == ChatRole.Assistant && message.Text.Contains("[Tool Calls]", StringComparison.Ordinal))
            .Select(message => message.Text)
            .ToList();

        compactedSummaries.Count.ShouldBeGreaterThan(0);
        compactedSummaries.ShouldContain(summary => summary.Contains("lookup_drink_recipe", StringComparison.Ordinal));
        compactedSummaries.ShouldContain(summary => summary.Contains("Recipe result 1", StringComparison.Ordinal));

        var recentToolCall = compacted.SelectMany(message => message.Contents)
            .OfType<FunctionCallContent>()
            .Single();
        var recentToolResult = compacted.SelectMany(message => message.Contents)
            .OfType<FunctionResultContent>()
            .Single();

        recentToolCall.CallId.ShouldBe("recipe-call-4");
        recentToolResult.CallId.ShouldBe("recipe-call-4");
    }
}
