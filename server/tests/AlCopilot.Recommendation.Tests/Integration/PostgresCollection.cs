using Xunit;

namespace AlCopilot.Recommendation.Tests.Integration;

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}
