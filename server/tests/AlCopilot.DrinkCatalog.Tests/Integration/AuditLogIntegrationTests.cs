using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class AuditLogIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM drink_catalog.audit_log_entries; DELETE FROM drink_catalog.\"ImportBatches\"; DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.domain_events;");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task SuccessfulMutations_PersistAuditEntries()
    {
        var auditRepository = new AuditLogEntryRepository(_db);
        var auditQueryService = new AuditLogQueryService(_db);
        var currentActorAccessor = new StubCurrentActorAccessor(new CurrentActor("manager-123", "manager@alcopilot.local", true));
        var auditWriter = new AuditLogWriter(auditRepository, currentActorAccessor);

        var createTagHandler = new CreateTagHandler(new TagRepository(_db), auditWriter, _db);
        await createTagHandler.Handle(new CreateTagCommand("Classic"), CancellationToken.None);

        var importBatchRepository = new ImportBatchRepository(_db);
        var strategyResolver = new ImportSourceStrategyResolver([new IbaCocktailsSnapshotImportSourceStrategy()]);
        var workflowService = new ImportBatchWorkflowService(
            new TagRepository(_db),
            new IngredientRepository(_db),
            new DrinkRepository(_db),
            new DrinkQueryService(_db));

        var createImportDraftHandler = new StartImportHandler(strategyResolver, importBatchRepository, workflowService, auditWriter, currentActorAccessor, _db);

        var draft = await createImportDraftHandler.Handle(
            new StartImportCommand(
                "iba-cocktails-snapshot",
                string.Empty,
                new ImportSourceInput(null, null, "application/json", [])),
            CancellationToken.None);

        var entries = await auditQueryService.GetRecentAsync();

        entries.ShouldContain(entry => entry.Action == "tag.create" && entry.SubjectType == "tag");
        entries.ShouldContain(entry => entry.Action == "import-batch.create" && entry.SubjectType == "import-batch");
        entries.ShouldContain(entry => entry.Action == "import-batch.validate" && entry.SubjectType == "import-batch");
        entries.ShouldAllBe(entry => entry.ActorUserId == "manager-123");
    }

    private sealed class StubCurrentActorAccessor(CurrentActor actor) : ICurrentActorAccessor
    {
        public CurrentActor GetCurrent() => actor;
    }
}
