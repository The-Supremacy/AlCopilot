using Xunit;

namespace AlCopilot.Recommendation.IntegrationTests.Integration;

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}
