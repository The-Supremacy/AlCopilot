using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.EvalTests.Eval;

internal static class RecommendationAgentLocalEvaluator
{
    public static IAgentEvaluator Create(
        RecommendationAgentEvalCase evalCase,
        IReadOnlyCollection<string> toolNames)
    {
        return Create(
            new RecommendationAgentEvalExpectations(
                evalCase.ExpectedResponseFragments,
                evalCase.ForbiddenResponseFragments,
                evalCase.ExpectedToolNames,
                evalCase.ForbiddenToolNames,
                evalCase.MaxToolCallCount,
                evalCase.ExpectedRecommendedDrinkNames,
                evalCase.ForbiddenRecommendedDrinkNames),
            toolNames);
    }

    public static IAgentEvaluator Create(
        RecommendationAgentEvalSessionTurn turn,
        IReadOnlyCollection<string> toolNames)
    {
        return Create(
            new RecommendationAgentEvalExpectations(
                turn.ExpectedResponseFragments,
                turn.ForbiddenResponseFragments,
                turn.ExpectedToolNames,
                turn.ForbiddenToolNames,
                turn.MaxToolCallCount,
                turn.ExpectedRecommendedDrinkNames,
                turn.ForbiddenRecommendedDrinkNames),
            toolNames);
    }

    private static IAgentEvaluator Create(
        RecommendationAgentEvalExpectations expectations,
        IReadOnlyCollection<string> toolNames)
    {
        return new LocalEvaluator(
            EvalChecks.NonEmpty(),
            CreateExpectedResponseFragmentsCheck(expectations),
            CreateForbiddenResponseFragmentsCheck(expectations),
            CreateExpectedToolNamesCheck(expectations, toolNames),
            CreateForbiddenToolNamesCheck(expectations, toolNames),
            CreateMaxToolCallCountCheck(expectations, toolNames),
            CreateExpectedRecommendedDrinkNamesCheck(expectations),
            CreateForbiddenRecommendedDrinkNamesCheck(expectations),
            CreateNoRepeatedToolCallsCheck(toolNames));
    }

    private static EvalCheck CreateExpectedResponseFragmentsCheck(RecommendationAgentEvalExpectations expectations)
    {
        return FunctionEvaluator.Create(
            "expected_response_fragments",
            item =>
            {
                var missing = expectations.ExpectedResponseFragments
                    .Where(fragment => !item.Response.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return new EvalCheckResult(
                    missing.Count == 0,
                    missing.Count == 0
                        ? "All expected response fragments were present."
                        : $"Missing expected response fragments: {string.Join(", ", missing)}",
                    "expected_response_fragments");
            });
    }

    private static EvalCheck CreateForbiddenResponseFragmentsCheck(RecommendationAgentEvalExpectations expectations)
    {
        return FunctionEvaluator.Create(
            "forbidden_response_fragments",
            item =>
            {
                var present = expectations.ForbiddenResponseFragments
                    .Where(fragment => item.Response.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return new EvalCheckResult(
                    present.Count == 0,
                    present.Count == 0
                        ? "No forbidden response fragments were present."
                        : $"Forbidden response fragments were present: {string.Join(", ", present)}",
                    "forbidden_response_fragments");
            });
    }

    private static EvalCheck CreateExpectedToolNamesCheck(
        RecommendationAgentEvalExpectations expectations,
        IReadOnlyCollection<string> toolNames)
    {
        return FunctionEvaluator.Create(
            "expected_tool_names",
            _ =>
            {
                var missing = expectations.ExpectedToolNames
                    .Where(expected => !toolNames.Any(toolName =>
                        string.Equals(toolName, expected, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                return new EvalCheckResult(
                    missing.Count == 0,
                    missing.Count == 0
                        ? "All expected tools were called."
                        : $"Missing expected tool calls: {string.Join(", ", missing)}",
                    "expected_tool_names");
            });
    }

    private static EvalCheck CreateForbiddenToolNamesCheck(
        RecommendationAgentEvalExpectations expectations,
        IReadOnlyCollection<string> toolNames)
    {
        return FunctionEvaluator.Create(
            "forbidden_tool_names",
            _ =>
            {
                var present = expectations.ForbiddenToolNames
                    .Where(forbidden => toolNames.Any(toolName =>
                        string.Equals(toolName, forbidden, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                return new EvalCheckResult(
                    present.Count == 0,
                    present.Count == 0
                        ? "No forbidden tools were called."
                        : $"Forbidden tools were called: {string.Join(", ", present)}",
                    "forbidden_tool_names");
            });
    }

    private static EvalCheck CreateMaxToolCallCountCheck(
        RecommendationAgentEvalExpectations expectations,
        IReadOnlyCollection<string> toolNames)
    {
        return FunctionEvaluator.Create(
            "max_tool_call_count",
            _ =>
            {
                var toolCallCount = toolNames.Count;
                return new EvalCheckResult(
                    toolCallCount <= expectations.MaxToolCallCount,
                    toolCallCount <= expectations.MaxToolCallCount
                        ? $"Tool call count {toolCallCount} stayed within limit {expectations.MaxToolCallCount}."
                        : $"Tool call count {toolCallCount} exceeded limit {expectations.MaxToolCallCount}.",
                    "max_tool_call_count");
            });
    }

    private static EvalCheck CreateNoRepeatedToolCallsCheck(
        IReadOnlyCollection<string> toolNames)
    {
        return FunctionEvaluator.Create(
            "no_repeated_tool_calls",
            _ =>
            {
                var repeated = toolNames
                    .GroupBy(toolName => toolName, StringComparer.OrdinalIgnoreCase)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToList();

                return new EvalCheckResult(
                    repeated.Count == 0,
                    repeated.Count == 0
                        ? "No tool names were repeated."
                        : $"Repeated tool calls: {string.Join(", ", repeated)}",
                    "no_repeated_tool_calls");
            });
    }

    private static EvalCheck CreateExpectedRecommendedDrinkNamesCheck(RecommendationAgentEvalExpectations expectations)
    {
        return FunctionEvaluator.Create(
            "expected_recommended_drink_names",
            item =>
            {
                var missing = expectations.ExpectedRecommendedDrinkNames
                    .Where(drinkName => !LooksRecommended(item.Response, drinkName))
                    .ToList();

                return new EvalCheckResult(
                    missing.Count == 0,
                    missing.Count == 0
                        ? "All expected drink recommendations were present."
                        : $"Missing expected drink recommendations: {string.Join(", ", missing)}",
                    "expected_recommended_drink_names");
            });
    }

    private static EvalCheck CreateForbiddenRecommendedDrinkNamesCheck(RecommendationAgentEvalExpectations expectations)
    {
        return FunctionEvaluator.Create(
            "forbidden_recommended_drink_names",
            item =>
            {
                var present = expectations.ForbiddenRecommendedDrinkNames
                    .Where(drinkName => LooksRecommended(item.Response, drinkName))
                    .ToList();

                return new EvalCheckResult(
                    present.Count == 0,
                    present.Count == 0
                        ? "No forbidden drink recommendations were present."
                        : $"Forbidden drink recommendations were present: {string.Join(", ", present)}",
                    "forbidden_recommended_drink_names");
            });
    }

    private static bool LooksRecommended(string response, string drinkName)
    {
        var escapedDrinkName = System.Text.RegularExpressions.Regex.Escape(drinkName);
        var patterns = new[]
        {
            $@"(?m)^\s*[-*]\s+{escapedDrinkName}\b",
            $@"(?m)^\s*\d+[\.)]\s+{escapedDrinkName}\b",
            $@"(?im)\brecommend(?:ation)?\s+is\s+(?:an?\s+|the\s+)?(?:\*\*)?{escapedDrinkName}\b(?:\*\*)?",
            $@"(?im)\brecommend(?:ed)?\s+(?:an?\s+|the\s+)?(?:\*\*)?{escapedDrinkName}\b(?:\*\*)?",
            $@"(?im)\btry\s+(?:an?\s+|the\s+)?(?:\*\*)?{escapedDrinkName}\b(?:\*\*)?",
            $@"(?im)\boption\b.{{0,80}}\bis\s+(?:an?\s+|the\s+)?(?:\*\*)?{escapedDrinkName}\b(?:\*\*)?",
            $@"(?im)\b(?:an?\s+|the\s+)?\*\*{escapedDrinkName}\*\*\s+(?:is|remains|would be|works|fits)\b",
        };

        return patterns.Any(pattern =>
            System.Text.RegularExpressions.Regex.Matches(
                    response,
                    pattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                .Any(match => !IsNegatedRecommendationMention(response, match.Index, match.Length)));
    }

    private static bool IsNegatedRecommendationMention(string response, int index, int length)
    {
        var start = Math.Max(0, index - 80);
        var end = Math.Min(response.Length, index + length + 120);
        var context = response[start..end];
        var negationMarkers = new[]
        {
            "off the table",
            "contains campari",
            "does contain campari",
            "do not recommend",
            "don't recommend",
            "not recommend",
            "can't recommend",
            "cannot recommend",
            "won't recommend",
            "so i'll stick with",
        };

        return negationMarkers.Any(marker => context.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record RecommendationAgentEvalExpectations(
        IReadOnlyList<string> ExpectedResponseFragments,
        IReadOnlyList<string> ForbiddenResponseFragments,
        IReadOnlyList<string> ExpectedToolNames,
        IReadOnlyList<string> ForbiddenToolNames,
        int MaxToolCallCount,
        IReadOnlyList<string> ExpectedRecommendedDrinkNames,
        IReadOnlyList<string> ForbiddenRecommendedDrinkNames);
}
