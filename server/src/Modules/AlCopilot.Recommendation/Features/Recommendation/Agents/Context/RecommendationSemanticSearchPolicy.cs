namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal static class RecommendationSemanticSearchPolicy
{
    internal static bool ShouldUseSemanticSearch(
        RecommendationRequestQueryAnalysis analysis,
        RecommendationRunInputs inputs)
    {
        var hasExactDrinkMention = !string.IsNullOrWhiteSpace(
            RecommendationCatalogMatcher.FindMentionedDrinkName(inputs.Drinks, analysis.NormalizedMessage));

        // Exact drink-details lookups do not benefit much from semantic search. Once we have a
        // concrete catalog drink mention, lexical resolution is a cheaper and more precise path.
        if (analysis.LooksLikeDrinkDetails && hasExactDrinkMention)
        {
            return false;
        }

        var mentionedIngredientNames = RecommendationCatalogMatcher.FindMentionedIngredientNames(
            inputs.Drinks,
            analysis.NormalizedMessage);

        // Ingredient-constrained recommendation prompts like "with gin and lime" are also usually
        // better served by deterministic filtering when the message does not include additional
        // descriptive cues such as "bright", "smoky", or "refreshing".
        if (!analysis.LooksLikeDrinkDetails
            && analysis.RequestDescriptors.Count == 0
            && mentionedIngredientNames.Count > 0)
        {
            return false;
        }

        return true;
    }
}
