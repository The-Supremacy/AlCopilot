using System.Text.Json;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationNarrationContextProvider : AIContextProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly IReadOnlyList<string> ProviderStateKeys = [RecommendationNarrationContextState.StateKey];

    public RecommendationNarrationContextProvider()
        : base(null, null, null)
    {
    }

    public override IReadOnlyList<string> StateKeys => ProviderStateKeys;

    protected override ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.Session is null ||
            !context.Session.StateBag.TryGetValue<RecommendationNarrationContextState>(
                RecommendationNarrationContextState.StateKey,
                out var state,
                SerializerOptions) ||
            state is null)
        {
            return ValueTask.FromResult(new AIContext());
        }

        var messages = RecommendationNarrationMessageBuilder.BuildContextMessages(
            new RecommendationNarrationContext(
                state.ProfileSummary,
                state.CandidateSummary));

        return ValueTask.FromResult(new AIContext
        {
            Messages = messages,
        });
    }

    internal static void SetContext(AgentSession session, RecommendationNarrationContext context)
    {
        session.StateBag.SetValue(
            RecommendationNarrationContextState.StateKey,
            new RecommendationNarrationContextState(
                context.ProfileSummary,
                context.CandidateSummary),
            SerializerOptions);
    }

    internal sealed record RecommendationNarrationContextState(
        string ProfileSummary,
        string CandidateSummary)
    {
        internal const string StateKey = "recommendation-narration-context";
    }
}
