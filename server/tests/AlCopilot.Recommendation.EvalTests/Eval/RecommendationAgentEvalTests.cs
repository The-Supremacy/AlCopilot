using Microsoft.Agents.AI;
using Shouldly;
using Xunit.Abstractions;

namespace AlCopilot.Recommendation.EvalTests.Eval;

public sealed class RecommendationAgentEvalTests(ITestOutputHelper output)
{
    public static TheoryData<RecommendationAgentEvalCase> Corpus
    {
        get
        {
            var corpus = RecommendationAgentEvalCorpus.Load();
            var data = new TheoryData<RecommendationAgentEvalCase>();
            foreach (var evalCase in corpus.Cases)
            {
                data.Add(evalCase);
            }

            return data;
        }
    }

    public static TheoryData<RecommendationAgentEvalSessionCase> SessionCorpus
    {
        get
        {
            var corpus = RecommendationAgentEvalCorpus.Load();
            var data = new TheoryData<RecommendationAgentEvalSessionCase>();
            foreach (var evalCase in corpus.SessionCases)
            {
                data.Add(evalCase);
            }

            return data;
        }
    }

    [Theory]
    [Trait("Category", "Eval")]
    [MemberData(nameof(Corpus))]
    public async Task CorpusCase_PassesLocalBehaviorChecks(RecommendationAgentEvalCase evalCase)
    {
        var corpus = RecommendationAgentEvalCorpus.Load();
        var harness = RecommendationAgentEvalHarness.Create(corpus);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));

        for (var repetition = 1; repetition <= Math.Max(1, evalCase.RepetitionCount); repetition++)
        {
            var result = await harness.RunAsync(evalCase, repetition, timeout.Token);

            WriteResult(result);

            var evaluator = RecommendationAgentLocalEvaluator.Create(evalCase, result.ToolInvocations);
            var evaluationResult = await evaluator.EvaluateAsync(
                [new EvalItem(evalCase.Prompt, result.Response)],
                $"recommendation-agent-eval-{evalCase.Name}",
                timeout.Token);

            AssertEvaluationPassed(evalCase.Name, result, evaluationResult);
        }
    }

    [Theory]
    [Trait("Category", "Eval")]
    [MemberData(nameof(SessionCorpus))]
    public async Task SessionCorpusCase_PassesLocalBehaviorChecks(RecommendationAgentEvalSessionCase evalCase)
    {
        var corpus = RecommendationAgentEvalCorpus.Load();
        var harness = RecommendationAgentEvalHarness.Create(corpus);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(4));

        var results = await harness.RunSessionAsync(evalCase, timeout.Token);

        results.Count.ShouldBe(evalCase.Turns.Count);
        results.Select(result => result.SessionId).Distinct().Count().ShouldBe(1);
        for (var index = 0; index < evalCase.Turns.Count; index++)
        {
            var turn = evalCase.Turns[index];
            var result = results[index];

            WriteResult(result);

            var evaluator = RecommendationAgentLocalEvaluator.Create(turn, result.ToolInvocations);
            var evaluationResult = await evaluator.EvaluateAsync(
                [new EvalItem(turn.Prompt, result.Response)],
                $"recommendation-agent-eval-session-{evalCase.Name}-turn-{result.TurnNumber}",
                timeout.Token);

            AssertEvaluationPassed(evalCase.Name, result, evaluationResult);
        }
    }

    private void WriteResult(RecommendationAgentEvalResult result)
    {
        output.WriteLine($"Case: {result.CaseName}");
        output.WriteLine($"Turn: {result.TurnNumber}");
        output.WriteLine($"Session: {result.SessionId}");
        output.WriteLine($"Response: {result.Response}");
        output.WriteLine(
            $"Tools: {string.Join(", ", result.ToolInvocations.Select(invocation => invocation.ToolName))}");
    }

    private static void AssertEvaluationPassed(
        string caseName,
        RecommendationAgentEvalResult result,
        AgentEvaluationResults evaluationResult)
    {
        var failureDetails = string.Join(
            "; ",
            evaluationResult.Items
                .SelectMany(item => item.Metrics.Values)
                .Where(metric => metric.Interpretation?.Failed == true)
                .Select(metric => $"{metric.Name}: {metric.Reason}"));

        evaluationResult.AllPassed.ShouldBeTrue(
            $"{caseName} turn {result.TurnNumber} failed local MAF evaluation metrics. {failureDetails} Response: {result.Response}");
    }
}
