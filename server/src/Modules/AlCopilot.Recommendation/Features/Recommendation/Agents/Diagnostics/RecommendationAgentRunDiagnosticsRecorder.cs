using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationAgentRunDiagnosticsRecorder(
    IRecommendationExecutionTraceRecorder executionTraceRecorder,
    IAgentMessageDiagnosticRepository diagnosticRepository,
    IHostEnvironment hostEnvironment,
    IOptions<RecommendationObservabilityOptions> observabilityOptions) : IRecommendationAgentRunDiagnosticsRecorder
{
    public void Record(ChatSession session, AgentRun agentRun, AgentResponse response)
    {
        var agentRunTraceStep = BuildAgentRunTraceStep(response);
        executionTraceRecorder.Record(agentRunTraceStep);

        var executionTrace = ShouldPersistExecutionTrace()
            ? executionTraceRecorder.Drain()
            : null;

        if (!string.IsNullOrWhiteSpace(agentRunTraceStep.Reasoning))
        {
            diagnosticRepository.Add(AgentMessageDiagnostic.Create(
                session.Id,
                agentRun.Id,
                null,
                "reasoning",
                "provider.reasoning",
                agentRunTraceStep.Reasoning,
                null));
        }

        foreach (var step in executionTrace ?? [])
        {
            diagnosticRepository.Add(AgentMessageDiagnostic.Create(
                session.Id,
                agentRun.Id,
                null,
                "trace",
                step.StepName,
                step.Summary,
                System.Text.Json.JsonSerializer.Serialize(step)));
        }
    }

    private static RecommendationExecutionTraceStep BuildAgentRunTraceStep(AgentResponse response)
    {
        var reasoning = string.Join(
            "\n\n",
            response.Messages
                .SelectMany(message => message.Contents)
                .OfType<TextReasoningContent>()
                .Select(content => content.Text)
                .Where(text => !string.IsNullOrWhiteSpace(text)));
        var usage = response.Usage;
        var finishReason = response.FinishReason?.ToString();

        return new RecommendationExecutionTraceStep(
            "agent.run",
            string.IsNullOrWhiteSpace(finishReason) ? "completed" : finishReason,
            "Recommendation narrator agent produced an assistant response.",
            DateTimeOffset.UtcNow,
            new Dictionary<string, string?>
            {
                ["finishReason"] = finishReason,
                ["inputTokens"] = usage?.InputTokenCount?.ToString(),
                ["outputTokens"] = usage?.OutputTokenCount?.ToString(),
                ["reasoningTokens"] = usage?.ReasoningTokenCount?.ToString(),
                ["messageCount"] = response.Messages.Count.ToString(),
            },
            [],
            string.IsNullOrWhiteSpace(reasoning) ? null : reasoning);
    }

    private bool ShouldPersistExecutionTrace()
    {
        return hostEnvironment.IsDevelopment()
            && observabilityOptions.Value.PersistExecutionTraceInDevelopment;
    }
}
