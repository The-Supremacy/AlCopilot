using Xunit;

namespace AlCopilot.CustomerProfile.Tests.Integration;

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}
