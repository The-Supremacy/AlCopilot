using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace AlCopilot.Recommendation.IntegrationTests.Integration;

[Trait("Category", "Integration")]
public sealed class RecommendationSemanticSearchIntegrationTests
{
    [Fact]
    public async Task SearchAsync_UsesPersistedCatalogVectorsAndQueriesLiveQdrant()
    {
        await using var container = new ContainerBuilder("qdrant/qdrant:v1.13.2")
            .WithPortBinding(6333, true)
            .WithPortBinding(6334, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilExternalTcpPortIsAvailable(6334)
                .UntilHttpRequestIsSucceeded(request => request.ForPort(6333).ForPath("/")))
            .Build();

        await container.StartAsync();

        var options = Options.Create(new RecommendationSemanticOptions
        {
            Enabled = true,
            QdrantEndpoint = $"http://{container.Hostname}:{container.GetMappedPublicPort(6334)}",
            CollectionName = $"recommendation-semantic-{Guid.NewGuid():N}",
            SearchLimit = 8,
            NameWeight = 1.25d,
            IngredientWeight = 1.0d,
            DescriptionWeight = 1.5d,
            EmbeddingModelId = "fake-test-model",
        });

        var vectorStore = new RecommendationQdrantVectorStore(options, NullLogger<RecommendationQdrantVectorStore>.Instance);
        var embeddingFactory = new FakeEmbeddingClientFactory();
        var indexingService = new RecommendationSemanticIndexingService(
            embeddingFactory,
            vectorStore,
            NullLogger<RecommendationSemanticIndexingService>.Instance);
        var service = new RecommendationSemanticSearchService(
            embeddingFactory,
            vectorStore,
            options,
            NullLogger<RecommendationSemanticSearchService>.Instance);

        var french75 = CreateDrink(
            Guid.Parse("00000000-0000-0000-0000-000000000510"),
            "French 75",
            "Sparkling, bright, and lightly sweet.",
            [CreateRecipeEntry(Guid.Parse("00000000-0000-0000-0000-000000000511"), "Gin"), CreateRecipeEntry(Guid.Parse("00000000-0000-0000-0000-000000000512"), "Prosecco")]);
        var negroni = CreateDrink(
            Guid.Parse("00000000-0000-0000-0000-000000000520"),
            "Negroni",
            "Bittersweet and spirit-forward.",
            [CreateRecipeEntry(Guid.Parse("00000000-0000-0000-0000-000000000521"), "Gin"), CreateRecipeEntry(Guid.Parse("00000000-0000-0000-0000-000000000522"), "Campari")]);

        var indexResult = await indexingService.ReplaceCatalogAsync([french75, negroni], CancellationToken.None);
        indexResult.PointCount.ShouldBeGreaterThan(0);

        var sparklingResult = await service.SearchAsync("I want a sparkly sweet drink", CancellationToken.None);
        sparklingResult.ByDrinkId.Keys.ShouldContain(french75.Id);
        sparklingResult.TopIngredientMatch?.DrinkName.ShouldBe("French 75");
        sparklingResult.Find(french75.Id)?.MatchedDescriptors.ShouldContain("Sparkling, bright, and lightly sweet.");

        var bittersweetResult = await service.SearchAsync("I want something bitter", CancellationToken.None);
        bittersweetResult.ByDrinkId.Keys.ShouldContain(negroni.Id);
        bittersweetResult.Find(negroni.Id)?.WeightedScore.ShouldBeGreaterThan(
            bittersweetResult.Find(french75.Id)?.WeightedScore ?? 0d);
        bittersweetResult.Find(negroni.Id)?.DrinkName.ShouldBe("Negroni");
    }

    private static DrinkDetailDto CreateDrink(Guid id, string name, string description, List<RecipeEntryDto> recipeEntries)
    {
        return new DrinkDetailDto(id, name, null, description, "Stir", null, null, [], recipeEntries);
    }

    private static RecipeEntryDto CreateRecipeEntry(Guid ingredientId, string ingredientName)
    {
        return new RecipeEntryDto(new IngredientDto(ingredientId, ingredientName, []), "1 oz", null);
    }

    private sealed class FakeEmbeddingClientFactory : IRecommendationEmbeddingClientFactory
    {
        public IRecommendationEmbeddingClient Create() => new FakeEmbeddingClient();
    }

    private sealed class FakeEmbeddingClient : IRecommendationEmbeddingClient
    {
        public Task<ReadOnlyMemory<float>> CreateEmbeddingAsync(string input, CancellationToken cancellationToken = default)
        {
            var vector = input switch
            {
                "French 75" => new float[] { 1.0f, 0.1f, 0.0f },
                "Sparkling, bright, and lightly sweet." => new float[] { 0.0f, 1.0f, 0.0f },
                "Gin" => new float[] { 0.1f, 0.0f, 0.0f },
                "Prosecco" => new float[] { 0.0f, 0.9f, 0.0f },
                "Negroni" => new float[] { 0.9f, 0.0f, 0.1f },
                "Bittersweet and spirit-forward." => new float[] { 0.0f, 0.0f, 1.0f },
                "Campari" => new float[] { 0.0f, 0.0f, 0.8f },
                "I want a sparkly sweet drink" => new float[] { 0.0f, 1.0f, 0.0f },
                "I want something bitter" => new float[] { 0.0f, 0.0f, 1.0f },
                _ => new float[] { 0.1f, 0.1f, 0.1f },
            };

            return Task.FromResult<ReadOnlyMemory<float>>(vector);
        }
    }
}
