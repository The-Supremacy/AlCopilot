using AlCopilot.Recommendation.Features.Recommendation;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.Handlers;

public sealed class RecommendationSemanticHitAggregatorTests
{
    [Fact]
    public void Aggregate_AppliesFacetWeightsAndSummarizesMatchesPerDrink()
    {
        var french75Id = Guid.Parse("00000000-0000-0000-0000-000000000301");
        var negroniId = Guid.Parse("00000000-0000-0000-0000-000000000302");
        var options = new RecommendationSemanticOptions
        {
            NameWeight = 1.25d,
            IngredientWeight = 1.0d,
            DescriptionWeight = 1.5d,
        };

        var result = RecommendationSemanticHitAggregator.Aggregate(
            [
                new RecommendationSemanticHit(Guid.NewGuid(), french75Id, "French 75", RecommendationSemanticFacetKind.Description, "sparkling", null, 0.80d),
                new RecommendationSemanticHit(Guid.NewGuid(), french75Id, "French 75", RecommendationSemanticFacetKind.Ingredient, "Prosecco", "Prosecco", 0.60d),
                new RecommendationSemanticHit(Guid.NewGuid(), french75Id, "French 75", RecommendationSemanticFacetKind.Name, "French 75", null, 0.40d),
                new RecommendationSemanticHit(Guid.NewGuid(), negroniId, "Negroni", RecommendationSemanticFacetKind.Description, "bittersweet", null, 0.70d),
            ],
            options);

        result.ByDrinkId.Count.ShouldBe(2);
        result.TopNameMatch?.DrinkName.ShouldBe("French 75");
        result.TopIngredientMatch?.DrinkName.ShouldBe("French 75");

        var french75 = result.Find(french75Id);
        french75.ShouldNotBeNull();
        french75.WeightedScore.ShouldBe(2.30d, 0.001d);
        french75.NameScore.ShouldBe(0.40d, 0.001d);
        french75.IngredientScore.ShouldBe(0.60d, 0.001d);
        french75.DescriptionScore.ShouldBe(0.80d, 0.001d);
        french75.MatchedFacets.ShouldBe(
            [
                RecommendationSemanticFacetKind.Description,
                RecommendationSemanticFacetKind.Ingredient,
                RecommendationSemanticFacetKind.Name,
            ]);
        french75.MatchedIngredients.ShouldBe(["Prosecco"]);
        french75.MatchedDescriptors.ShouldBe(["sparkling"]);
        french75.SummaryHints.ShouldBe(["French 75", "Prosecco", "sparkling"]);
    }
}
