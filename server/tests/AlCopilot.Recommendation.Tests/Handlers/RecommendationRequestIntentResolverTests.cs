using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Handlers;

public sealed class RecommendationRequestIntentResolverTests
{
    private readonly RecommendationRequestIntentResolver resolver = new();

    [Fact]
    public void Resolve_ReturnsIngredientDiscovery_WhenMessageMentionsKnownIngredient()
    {
        var intent = resolver.Resolve(
            "Suggest me a drink with tequila",
            new RecommendationRunInputs(
                new CustomerProfileDto([], [], [], []),
                [CreateDrink("Long Island Iced Tea", "Tequila", "Vodka")]));

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.IngredientDiscovery);
        intent.RequestedIngredientName.ShouldBe("Tequila");
    }

    [Fact]
    public void Resolve_ReturnsRecipeLookup_WhenMessageMentionsKnownDrink()
    {
        var intent = resolver.Resolve(
            "How do I make a Negroni?",
            new RecommendationRunInputs(
                new CustomerProfileDto([], [], [], []),
                [CreateDrink("Negroni", "Gin", "Campari")]));

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.RecipeLookup);
        intent.RequestedDrinkName.ShouldBe("Negroni");
    }

    [Fact]
    public void Resolve_CollectsPreferenceSignals_WhenMessageUsesKnownDescriptors()
    {
        var intent = resolver.Resolve(
            "I want something sweet and sparkling",
            new RecommendationRunInputs(
                new CustomerProfileDto([], [], [], []),
                [CreateDrink("French 75", "Gin", "Champagne")]));

        intent.Kind.ShouldBe(RecommendationRequestIntentKind.Recommendation);
        intent.PreferenceSignals.ShouldBe(["sparkling", "sweet"]);
    }

    private static DrinkDetailDto CreateDrink(string name, params string[] ingredientNames)
    {
        return new DrinkDetailDto(
            Guid.NewGuid(),
            name,
            null,
            $"{name} description",
            null,
            null,
            null,
            [],
            ingredientNames
                .Select(ingredientName => new RecipeEntryDto(
                    new IngredientDto(Guid.NewGuid(), ingredientName, []),
                    "1 oz",
                    null))
                .ToList());
    }
}
