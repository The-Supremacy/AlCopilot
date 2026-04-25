using Xunit;

namespace AlCopilot.CustomerProfile.IntegrationTests.Integration;

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}
