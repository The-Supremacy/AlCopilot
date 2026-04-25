using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationInvocationContextProvider(
    Guid recommendationSessionId) : AIContextProvider
{
    internal static readonly ProviderSessionState<RecommendationInvocationProviderState> SessionState = new(
        _ => new RecommendationInvocationProviderState(),
        "recommendation.invocation");

    public override IReadOnlyList<string> StateKeys =>
    [
        SessionState.StateKey,
    ];

    protected override ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var customerMessage = context.AIContext.Messages?
            .Where(message => message.Role == ChatRole.User)
            .Select(message => message.Text)
            .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));
        var state = SessionState.GetOrInitializeState(context.Session);

        state.RecommendationSessionId = recommendationSessionId;
        state.CustomerMessage = customerMessage;
        state.RequestAnalysis = string.IsNullOrWhiteSpace(customerMessage)
            ? null
            : RecommendationRequestQueryAnalyzer.Analyze(customerMessage);
        SessionState.SaveState(context.Session, state);

        return ValueTask.FromResult(new AIContext());
    }
}

internal sealed class RecommendationInvocationProviderState
{
    public Guid RecommendationSessionId { get; set; }

    public string? CustomerMessage { get; set; }

    public RecommendationRequestQueryAnalysis? RequestAnalysis { get; set; }
}
