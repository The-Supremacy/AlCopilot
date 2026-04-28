namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

public sealed class RecommendationCompactionOptions
{
    public const string SectionName = "Recommendation:Compaction";

    public bool Enabled { get; init; } = true;

    public int ToolResultGroupsThreshold { get; init; } = 32;

    public int ToolResultTokenThreshold { get; init; } = 96_000;

    public int MinimumPreservedGroups { get; init; } = 8;
}
