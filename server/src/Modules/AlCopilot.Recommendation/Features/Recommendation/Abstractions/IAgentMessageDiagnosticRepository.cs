namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IAgentMessageDiagnosticRepository
{
    void Add(AgentMessageDiagnostic diagnostic);
}
