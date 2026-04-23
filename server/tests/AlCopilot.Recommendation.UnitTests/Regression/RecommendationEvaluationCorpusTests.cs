using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Mediator;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Evaluations;

public sealed class RecommendationEvaluationCorpusTests
{
    public static TheoryData<RecommendationEvaluationCase> Corpus =>
    [
        new RecommendationEvaluationCase(
            "Make now from bar",
            "what can I make right now from my bar?",
            new CustomerProfileDto([], [], [], [IngredientIds.Gin, IngredientIds.Lime]),
            BuildCatalog(),
            ["Gimlet"],
            ["Martini", "Negroni"],
            [],
            ["owned: Gin, Lime", "Gimlet", "owned Gin, Lime", "missing none"]),
        new RecommendationEvaluationCase(
            "Near miss explains missing ingredient",
            "I want something bitter",
            new CustomerProfileDto([], [], [], [IngredientIds.Gin, IngredientIds.SweetVermouth]),
            BuildCatalog(),
            ["Martini"],
            ["Negroni", "Gimlet"],
            [],
            ["Negroni", "missing Campari", "owned Gin, Sweet Vermouth"]),
        new RecommendationEvaluationCase(
            "Prohibited ingredient exclusion",
            "suggest something classic",
            new CustomerProfileDto([], [], [IngredientIds.Campari], [IngredientIds.Gin, IngredientIds.SweetVermouth]),
            BuildCatalog(),
            ["Martini"],
            ["Gimlet"],
            ["Negroni"],
            ["prohibited: Campari", "Martini"]),
        new RecommendationEvaluationCase(
            "Light sparkling prosecco request",
            "I'm looking for a light sparkling drink, maybe with Prosecco?",
            new CustomerProfileDto([], [], [], [IngredientIds.Gin, IngredientIds.Lime]),
            BuildSparklingCatalog(),
            [],
            ["French 75"],
            ["Negroni"],
            ["requested ingredient: Prosecco", "preference signals: light, sparkling", "French 75", "missing Prosecco"])
    ];

    [Theory]
    [MemberData(nameof(Corpus))]
    public async Task Corpus_RemainsStableForDeterministicCandidatesAndRunContext(RecommendationEvaluationCase evaluationCase)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCustomerProfileQuery>(), Arg.Any<CancellationToken>())
            .Returns(evaluationCase.Profile);
        mediator.Send(Arg.Any<GetRecommendationCatalogQuery>(), Arg.Any<CancellationToken>())
            .Returns(evaluationCase.Drinks.ToList());

        var runInputsQueryService = new RecommendationRunInputsQueryService(mediator);
        var service = new RecommendationRunContextService(
            runInputsQueryService,
            new StubSemanticSearchService(),
            new RecommendationRequestIntentResolver(
                new StubCatalogFuzzyLookupService(),
                Options.Create(new RecommendationSemanticOptions())),
            new DeterministicRecommendationCandidateBuilder(),
            new RecommendationRunContextBuilder(),
            new RecommendationExecutionTraceRecorder());

        var runContext = await service.CreateAsync(evaluationCase.Prompt, CancellationToken.None);
        var prompt = RecommendationRunContextMessageBuilder.Build(runContext);

        runContext.RecommendationGroups.Single(group => group.Key == "make-now").Items.Select(item => item.DrinkName)
            .ShouldBe(evaluationCase.ExpectedMakeNow);
        runContext.RecommendationGroups.Single(group => group.Key == "buy-next").Items.Select(item => item.DrinkName)
            .ShouldBe(evaluationCase.ExpectedBuyNext);

        foreach (var forbiddenDrink in evaluationCase.ForbiddenDrinks)
        {
            runContext.RecommendationGroups.SelectMany(group => group.Items).Select(item => item.DrinkName)
                .ShouldNotContain(forbiddenDrink);
        }

        foreach (var requiredFragment in evaluationCase.RequiredPromptFragments)
        {
            prompt.ShouldContain(requiredFragment, customMessage: $"{evaluationCase.Name} should contain '{requiredFragment}'.");
        }
    }

    private static List<DrinkDetailDto> BuildCatalog()
    {
        return
        [
            CreateDrink(
                "Gimlet",
                "Bright and citrusy",
                "Shake",
                null,
                [CreateRecipeEntry(IngredientIds.Gin, "Gin"), CreateRecipeEntry(IngredientIds.Lime, "Lime")]),
            CreateDrink(
                "Martini",
                "Spirit-forward and aromatic",
                "Stir",
                "Lemon twist",
                [CreateRecipeEntry(IngredientIds.Gin, "Gin"), CreateRecipeEntry(IngredientIds.SweetVermouth, "Sweet Vermouth")]),
            CreateDrink(
                "Negroni",
                "Bittersweet and spirit-forward",
                "Stir",
                "Orange twist",
                [
                    CreateRecipeEntry(IngredientIds.Gin, "Gin"),
                    CreateRecipeEntry(IngredientIds.Campari, "Campari"),
                    CreateRecipeEntry(IngredientIds.SweetVermouth, "Sweet Vermouth")
                ]),
        ];
    }

    private static List<DrinkDetailDto> BuildSparklingCatalog()
    {
        return
        [
            CreateDrink(
                "French 75",
                "Light, sparkling, and bright.",
                "Shake and top",
                "Lemon twist",
                [
                    CreateRecipeEntry(IngredientIds.Gin, "Gin"),
                    CreateRecipeEntry(IngredientIds.Lime, "Lime"),
                    CreateRecipeEntry(IngredientIds.Prosecco, "Prosecco")
                ]),
            CreateDrink(
                "Negroni",
                "Bittersweet and spirit-forward",
                "Stir",
                "Orange twist",
                [
                    CreateRecipeEntry(IngredientIds.Gin, "Gin"),
                    CreateRecipeEntry(IngredientIds.Campari, "Campari"),
                    CreateRecipeEntry(IngredientIds.SweetVermouth, "Sweet Vermouth")
                ]),
        ];
    }

    private static DrinkDetailDto CreateDrink(
        string name,
        string description,
        string? method,
        string? garnish,
        List<RecipeEntryDto> recipeEntries)
    {
        return new DrinkDetailDto(Guid.NewGuid(), name, null, description, method, garnish, null, [], recipeEntries);
    }

    private static RecipeEntryDto CreateRecipeEntry(Guid ingredientId, string ingredientName)
    {
        return new RecipeEntryDto(new IngredientDto(ingredientId, ingredientName, []), "1 oz", null);
    }

    private static class IngredientIds
    {
        internal static readonly Guid Gin = Guid.Parse("00000000-0000-0000-0000-000000000101");
        internal static readonly Guid Lime = Guid.Parse("00000000-0000-0000-0000-000000000102");
        internal static readonly Guid SweetVermouth = Guid.Parse("00000000-0000-0000-0000-000000000103");
        internal static readonly Guid Campari = Guid.Parse("00000000-0000-0000-0000-000000000104");
        internal static readonly Guid Prosecco = Guid.Parse("00000000-0000-0000-0000-000000000105");
    }

    public sealed record RecommendationEvaluationCase(
        string Name,
        string Prompt,
        CustomerProfileDto Profile,
        IReadOnlyCollection<DrinkDetailDto> Drinks,
        IReadOnlyList<string> ExpectedMakeNow,
        IReadOnlyList<string> ExpectedBuyNext,
        IReadOnlyList<string> ForbiddenDrinks,
        IReadOnlyList<string> RequiredPromptFragments)
    {
        public override string ToString() => Name;
    }

    private sealed class StubSemanticSearchService : IRecommendationSemanticSearchService
    {
        public Task<RecommendationSemanticSearchResult> SearchAsync(
            string customerMessage,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RecommendationSemanticSearchResult.Empty);
        }
    }

    private sealed class StubCatalogFuzzyLookupService : IRecommendationCatalogFuzzyLookupService
    {
        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindDrinkMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<RecommendationFuzzyMatch>>([]);

        public Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindIngredientMatchesAsync(
            string searchText,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<RecommendationFuzzyMatch>>([]);
    }
}
