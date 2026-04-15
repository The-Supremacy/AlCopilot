using AlCopilot.CustomerProfile.Data;
using AlCopilot.Testing.Shared;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlCopilot.CustomerProfile.Tests.Integration;

public sealed class PostgresFixture : PostgreSqlContainerFixture, IAsyncLifetime
{
    public CustomerProfileDbContext CreateDbContext()
    {
        var services = new ServiceCollection();
        services.AddDomainEventAssembly(typeof(AlCopilot.CustomerProfile.CustomerProfileModule).Assembly);
        services.AddScoped<DomainEventInterceptor>();
        var serviceProvider = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<CustomerProfileDbContext>()
            .UseNpgsql(ConnectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "customer_profile"))
            .AddInterceptors(serviceProvider.GetRequiredService<DomainEventInterceptor>())
            .Options;

        return new CustomerProfileDbContext(options);
    }

    protected override async Task InitializeDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    Task IAsyncLifetime.InitializeAsync() => InitializeContainerAsync();

    Task IAsyncLifetime.DisposeAsync() => DisposeContainerAsync();
}
