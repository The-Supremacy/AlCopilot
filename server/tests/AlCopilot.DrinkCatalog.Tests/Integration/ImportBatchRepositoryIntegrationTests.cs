using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class ImportBatchRepositoryIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"ImportBatches\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task CreateBatch_PersistsProvenance()
    {
        var repository = new ImportBatchRepository(_db);
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            new ImportProvenance(
                "uploads/catalog.csv",
                "catalog.csv",
                "text/csv",
                new Dictionary<string, string?> { ["uploadedBy"] = "manager@alcopilot.com" }),
            new NormalizedCatalogImport([], [], []));

        repository.Add(batch);
        await _db.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(batch.Id);

        loaded.ShouldNotBeNull();
        loaded!.Status.ShouldBe(ImportBatchStatus.InProgress);
        loaded.Provenance.DisplayName.ShouldBe("catalog.csv");
        loaded.Provenance.Metadata["uploadedBy"].ShouldBe("manager@alcopilot.com");
    }

    [Fact]
    public async Task LifecycleState_PersistsDiagnosticsAuditAndSummaries()
    {
        var repository = new ImportBatchRepository(_db);
        var batch = ImportBatch.Create(ImportStrategyKey.IbaCocktailsSnapshot, ImportProvenance.Empty, new NormalizedCatalogImport([], [], []));
        repository.Add(batch);
        await _db.SaveChangesAsync();

        batch.RecordReviewedSnapshot(new ImportBatchProcessingResult(
            [new ImportDiagnostic(12, "name-normalized", "Name normalized.", "info")],
            new ImportReviewSummary(3, 1, 2),
            [new ImportReviewRow("drink", "mai-tai", "create", "Create drink 'Mai Tai'.", false, false)]));
        batch.MarkCompleted(
            new ImportApplySummary(3, 1, 1, 1),
            [new ImportDecisionAuditEntry("drink", "mai-tai", "reject", "manual override", "manager-123", "manager@alcopilot.local", DateTimeOffset.UtcNow)]);
        await _db.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(batch.Id);

        loaded.ShouldNotBeNull();
        loaded!.Status.ShouldBe(ImportBatchStatus.Completed);
        loaded.ValidatedAtUtc.ShouldNotBeNull();
        loaded.ReviewedAtUtc.ShouldNotBeNull();
        loaded.AppliedAtUtc.ShouldNotBeNull();
        loaded.Diagnostics.ShouldHaveSingleItem().Code.ShouldBe("name-normalized");
        loaded.DecisionAuditTrail.ShouldHaveSingleItem().Decision.ShouldBe("reject");
        loaded.DecisionAuditTrail.Single().ActorUserId.ShouldBe("manager-123");
        loaded.ReviewSummary.ShouldNotBeNull();
        loaded.ReviewSummary.CreateCount.ShouldBe(3);
        loaded.ReviewRows.ShouldHaveSingleItem().TargetKey.ShouldBe("mai-tai");
        loaded.ApplySummary.ShouldNotBeNull();
        loaded.ApplySummary.RejectedCount.ShouldBe(1);
    }

}
