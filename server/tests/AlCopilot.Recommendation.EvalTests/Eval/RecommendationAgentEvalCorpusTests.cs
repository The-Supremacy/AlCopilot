using Shouldly;

namespace AlCopilot.Recommendation.EvalTests.Eval;

public sealed class RecommendationAgentEvalCorpusTests
{
    private static readonly string[] KnownToolNames =
    [
        "search_drinks",
        "lookup_drink_recipe",
        "lookup_drinks_by_ingredient",
    ];

    [Fact]
    [Trait("Category", "EvalCorpus")]
    public void Corpus_IsWellFormed()
    {
        var corpus = RecommendationAgentEvalCorpus.Load();
        var catalog = RecommendationAgentEvalSeedCatalog.Create();

        corpus.Cases.Select(evalCase => evalCase.Name)
            .Concat(corpus.SessionCases.Select(evalCase => evalCase.Name))
            .ShouldBeUnique();

        foreach (var evalCase in corpus.Cases)
        {
            evalCase.Name.ShouldNotBeNullOrWhiteSpace();
            evalCase.Prompt.ShouldNotBeNullOrWhiteSpace();
            evalCase.MaxToolCallCount.ShouldBeGreaterThanOrEqualTo(0);
            evalCase.RepetitionCount.ShouldBeGreaterThanOrEqualTo(1);

            AssertProfileReferencesKnownIngredients(evalCase.Name, evalCase.Profile, catalog);
            AssertExpectedToolsAreKnown(evalCase.Name, evalCase.ExpectedToolNames);
            AssertExpectedToolsAreKnown(evalCase.Name, evalCase.ForbiddenToolNames);
            AssertRecommendedDrinksAreKnown(evalCase.Name, evalCase.ExpectedRecommendedDrinkNames, catalog);
            AssertRecommendedDrinksAreKnown(evalCase.Name, evalCase.ForbiddenRecommendedDrinkNames, catalog);
        }

        foreach (var evalCase in corpus.SessionCases)
        {
            evalCase.Name.ShouldNotBeNullOrWhiteSpace();
            evalCase.Turns.Count.ShouldBeGreaterThanOrEqualTo(2);

            AssertProfileReferencesKnownIngredients(evalCase.Name, evalCase.Profile, catalog);

            foreach (var turn in evalCase.Turns)
            {
                turn.Prompt.ShouldNotBeNullOrWhiteSpace();
                turn.MaxToolCallCount.ShouldBeGreaterThanOrEqualTo(0);
                AssertExpectedToolsAreKnown(evalCase.Name, turn.ExpectedToolNames);
                AssertExpectedToolsAreKnown(evalCase.Name, turn.ForbiddenToolNames);
                AssertRecommendedDrinksAreKnown(evalCase.Name, turn.ExpectedRecommendedDrinkNames, catalog);
                AssertRecommendedDrinksAreKnown(evalCase.Name, turn.ForbiddenRecommendedDrinkNames, catalog);
            }
        }
    }

    private static void AssertProfileReferencesKnownIngredients(
        string caseName,
        RecommendationAgentEvalProfile profile,
        RecommendationAgentEvalSeedCatalog catalog)
    {
        var ingredientNames = profile.OwnedIngredientNames
            .Concat(profile.DislikedIngredientNames)
            .Concat(profile.ProhibitedIngredientNames)
            .ToList();

        foreach (var ingredientName in ingredientNames)
        {
            catalog.FindIngredientId(ingredientName)
                .ShouldNotBeNull($"Eval case '{caseName}' references unknown ingredient '{ingredientName}'.");
        }
    }

    private static void AssertExpectedToolsAreKnown(
        string caseName,
        IReadOnlyCollection<string> toolNames)
    {
        foreach (var toolName in toolNames)
        {
            KnownToolNames.ShouldContain(
                known => string.Equals(known, toolName, StringComparison.OrdinalIgnoreCase),
                $"Eval case '{caseName}' references unknown tool '{toolName}'.");
        }
    }

    private static void AssertRecommendedDrinksAreKnown(
        string caseName,
        IReadOnlyCollection<string> drinkNames,
        RecommendationAgentEvalSeedCatalog catalog)
    {
        foreach (var drinkName in drinkNames)
        {
            catalog.Drinks.ShouldContain(
                drink => string.Equals(drink.Name, drinkName, StringComparison.OrdinalIgnoreCase),
                $"Eval case '{caseName}' references unknown recommended drink '{drinkName}'.");
        }
    }
}
