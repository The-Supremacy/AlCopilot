using Testcontainers.PostgreSql;

namespace AlCopilot.Testing.Shared;

public abstract class PostgreSqlContainerFixture
{
    private readonly PostgreSqlContainer _container;

    protected PostgreSqlContainerFixture()
        : this("postgres:17-alpine")
    {
    }

    protected PostgreSqlContainerFixture(string imageName)
    {
        _container = new PostgreSqlBuilder(imageName).Build();
    }

    protected string ConnectionString => _container.GetConnectionString();

    protected PostgreSqlContainer Container => _container;

    protected async Task InitializeContainerAsync()
    {
        await _container.StartAsync();
        await InitializeDatabaseAsync();
    }

    protected async Task DisposeContainerAsync()
    {
        await _container.DisposeAsync();
    }

    protected virtual Task InitializeDatabaseAsync() => Task.CompletedTask;
}
