using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class AgentFrameworkRecommendationNarrator(
    AIAgent agent,
    ILogger<AgentFrameworkRecommendationNarrator> logger) : IRecommendationNarrator
{
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
        var session = await CreateOrRestoreSessionAsync(
            agent,
            request.Session,
            cancellationToken);
        var context = RecommendationNarrationMessageBuilder.CreateContext(request);
        RecommendationNarrationContextProvider.SetContext(session, context);

        var response = await agent.RunAsync(
            request.CustomerMessage,
            session,
            options: null,
            cancellationToken);

        var content = string.IsNullOrWhiteSpace(response.Text)
            ? response.Messages.LastOrDefault(message => message.Role == ChatRole.Assistant)?.Text
            : response.Text;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Recommendation LLM returned an empty assistant message.");
        }

        var serializedSession = await agent.SerializeSessionAsync(
            session,
            cancellationToken: cancellationToken);

        return new RecommendationNarrationResult(
            content.Trim(),
            [],
            serializedSession.GetRawText());
    }

    private static async Task<AgentSession> CreateOrRestoreSessionAsync(
        AIAgent agent,
        ChatSession session,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(session.AgentSessionStateJson))
        {
            return await agent.CreateSessionAsync(cancellationToken);
        }

        using var document = System.Text.Json.JsonDocument.Parse(session.AgentSessionStateJson);
        return await agent.DeserializeSessionAsync(document.RootElement.Clone(), cancellationToken: cancellationToken);
    }
}
