using System.Net;
using System.Net.Http.Json;
using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Domain;
using AlCopilot.Testing.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AlCopilot.Host.IntegrationTests;

[Trait("Category", "Integration")]
[Collection("HostPostgres")]
public sealed class ManagementImportWorkflowTests(ManagementImportWorkflowFixture fixture) : IAsyncLifetime
{
    private readonly ManagementImportWorkflowFixture _fixture = fixture;

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CancelImport_PersistsCancelledStatusAcrossHttpReads()
    {
        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Add(ManagementAuthenticationTests.TestAuthHandler.UserHeaderName, "manager@alcopilot.local");
        client.DefaultRequestHeaders.Add(ManagementAuthenticationTests.TestAuthHandler.RolesHeaderName, "manager,user");

        var startResponse = await client.PostAsJsonAsync(
            "/api/drink-catalog/imports/",
            new StartImportCommand(
                "iba-cocktails-snapshot",
                string.Empty,
                new ImportSourceInput(null, null, "application/json", [])));

        startResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var started = await startResponse.Content.ReadFromJsonAsync<ImportBatchDto>();
        started.ShouldNotBeNull();
        started!.Status.ShouldBe("InProgress");

        var cancelResponse = await client.PostAsync($"/api/drink-catalog/imports/{started.Id}/cancel", null);

        cancelResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<ImportBatchDto>();
        cancelled.ShouldNotBeNull();
        cancelled!.Id.ShouldBe(started.Id);
        cancelled.Status.ShouldBe("Cancelled");

        var byIdResponse = await client.GetAsync($"/api/drink-catalog/imports/{started.Id}");
        byIdResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var byId = await byIdResponse.Content.ReadFromJsonAsync<ImportBatchDto>();
        byId.ShouldNotBeNull();
        byId!.Status.ShouldBe("Cancelled");

        var historyResponse = await client.GetAsync("/api/drink-catalog/imports/history");
        historyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var history = await historyResponse.Content.ReadFromJsonAsync<List<ImportBatchDto>>();
        history.ShouldNotBeNull();
        history.ShouldContain(batch => batch.Id == started.Id && batch.Status == "Cancelled");
    }

    private async Task ResetDatabaseAsync()
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DrinkCatalogDbContext>();
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM drink_catalog.audit_log_entries; DELETE FROM drink_catalog.\"ImportBatches\"; DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.domain_events;");
    }
}

[CollectionDefinition("HostPostgres")]
public sealed class HostPostgresCollection : ICollectionFixture<ManagementImportWorkflowFixture>;

public sealed class ManagementImportWorkflowFixture : PostgreSqlContainerFixture, IAsyncLifetime
{
    public ManagementAuthWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await InitializeContainerAsync();
        Factory = new ManagementAuthWebApplicationFactory(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        Factory.Dispose();
        await DisposeContainerAsync();
    }

    protected override async Task InitializeDatabaseAsync()
    {
        var services = new ServiceCollection();
        services.AddDomainEventAssembly(typeof(DrinkCreatedEvent).Assembly);
        services.AddScoped<DomainEventInterceptor>();
        var sp = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<DrinkCatalogDbContext>()
            .UseNpgsql(ConnectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "drink_catalog"))
            .AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>())
            .Options;

        await using var db = new DrinkCatalogDbContext(options);
        await db.Database.MigrateAsync();
    }

    public sealed class ManagementAuthWebApplicationFactory(string connectionString)
        : BackendIntegrationWebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:drink-catalog", connectionString);
            base.ConfigureWebHost(builder);
        }

        protected override IReadOnlyDictionary<string, string?> CreateConfigurationOverrides() =>
            new Dictionary<string, string?>
            {
                ["Authentication:Management:Authority"] = "http://localhost:8080/realms/alcopilot",
                ["Authentication:Management:ClientId"] = "alcopilot-management-portal",
                ["Authentication:Management:ClientSecret"] = "alcopilot-management-dev-secret",
            };

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = ManagementAuthenticationTests.TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = ManagementAuthenticationTests.TestAuthHandler.SchemeName;
                    options.DefaultForbidScheme = ManagementAuthenticationTests.TestAuthHandler.SchemeName;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, ManagementAuthenticationTests.TestAuthHandler>(
                    ManagementAuthenticationTests.TestAuthHandler.SchemeName,
                    _ => { });
        }
    }
}
