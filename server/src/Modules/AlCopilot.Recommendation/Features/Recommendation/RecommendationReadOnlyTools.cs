using System.ComponentModel;
using AlCopilot.Recommendation.Contracts.DTOs;
using Microsoft.SemanticKernel;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class RecommendationReadOnlyTools
{
    [KernelFunction("get_customer_profile_snapshot")]
    [Description("Reads the current customer profile snapshot used to shape recommendations.")]
    public string GetCustomerProfileSnapshot(string summary) => summary;

    [KernelFunction("get_candidate_snapshot")]
    [Description("Reads the bounded deterministic recommendation candidate snapshot.")]
    public string GetCandidateSnapshot(string summary) => summary;

    [KernelFunction("get_ingredient_gap_analysis")]
    [Description("Reads which missing ingredients separate make-now drinks from buy-next drinks.")]
    public string GetIngredientGapAnalysis(string summary) => summary;
}
