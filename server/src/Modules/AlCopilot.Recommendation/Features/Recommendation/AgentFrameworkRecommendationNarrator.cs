using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class AgentFrameworkRecommendationNarrator(
    IRecommendationAgentFactory agentFactory,
    ILogger<AgentFrameworkRecommendationNarrator> logger) : IRecommendationNarrator
{
    internal const string BartenderInstructions =
        """
        You are an experienced bartender.
        Base your answer only on the provided customer context and deterministic recommendation candidates.
        Prefer concise, practical guidance.
        Do not invent unavailable drinks or ignore prohibited ingredients.
        """;

    private static readonly RecommendationAgentDefinition NarratorDefinition = new(
        Name: "recommendation-narrator",
        Description: "Turns deterministic recommendation candidates into a concise bartender-style response.",
        Instructions: AgentFrameworkRecommendationNarrator.BartenderInstructions);

    public async Task<RecommendationNarrationResult> NarrateAsync(
        RecommendationNarrationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await RunNarrationAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Recommendation narration failed. The customer portal requires a working configured LLM provider.");
            throw;
        }
    }

    private async Task<RecommendationNarrationResult> RunNarrationAsync(
        RecommendationNarrationRequest request,
        CancellationToken cancellationToken)
    {
        var runtime = agentFactory.Create(
            NarratorDefinition,
            request.ContextInstructions,
            request.Session);
        var session = await runtime.Agent.CreateSessionAsync(
            request.Session.Id.ToString(),
            cancellationToken);
        var response = await runtime.Agent.RunAsync(
            request.CustomerMessage,
            session,
            new ChatClientAgentRunOptions(runtime.ChatOptions),
            cancellationToken);

        var content = string.IsNullOrWhiteSpace(response.Text)
            ? response.Messages.LastOrDefault(message => message.Role == ChatRole.Assistant)?.Text
            : response.Text;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Recommendation LLM returned an empty assistant message.");
        }

        return new RecommendationNarrationResult(content.Trim(), []);
    }
}
