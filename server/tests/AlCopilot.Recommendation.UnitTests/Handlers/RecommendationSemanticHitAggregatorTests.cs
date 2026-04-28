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
            DescriptionMinScore = 0.55d,
            DescriptionWeight = 1.5d,
        };

        var result = RecommendationSemanticHitAggregator.Aggregate(
            [
                new RecommendationSemanticHit(Guid.NewGuid(), french75Id, "French 75", RecommendationSemanticFacetKind.Description, "sparkling", null, 0.80d),
                new RecommendationSemanticHit(Guid.NewGuid(), french75Id, "French 75", RecommendationSemanticFacetKind.Description, "lightly sweet", null, 0.60d),
                new RecommendationSemanticHit(Guid.NewGuid(), french75Id, "French 75", RecommendationSemanticFacetKind.Description, "tail noise", null, 0.40d),
                new RecommendationSemanticHit(Guid.NewGuid(), negroniId, "Negroni", RecommendationSemanticFacetKind.Description, "bittersweet", null, 0.70d),
            ],
            options);

        result.ByDrinkId.Count.ShouldBe(2);

        var french75 = result.Find(french75Id);
        french75.ShouldNotBeNull();
        french75.WeightedScore.ShouldBe(2.10d, 0.001d);
        french75.DescriptionMatches.Select(match => match.Text).ShouldBe(["sparkling", "lightly sweet"]);
        french75.DescriptionMatches.Select(match => match.Score).ShouldBe([0.80d, 0.60d]);
        french75.SummaryHints.ShouldBe(["lightly sweet", "sparkling"]);
    }
}
