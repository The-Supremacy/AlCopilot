using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportBatch;
using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class ImportWorkflowIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;
    private ImportBatchRepository _importBatchRepository = null!;
    private AuditLogEntryRepository _auditLogEntryRepository = null!;
    private AuditLogWriter _auditLogWriter = null!;
    private ICurrentActorAccessor _currentActorAccessor = null!;
    private TagRepository _tagRepository = null!;
    private IngredientRepository _ingredientRepository = null!;
    private DrinkRepository _drinkRepository = null!;
    private DrinkQueryService _drinkQueryService = null!;
    private ImportBatchProcessingService _workflowService = null!;
    private IImportBatchApplyService _applyService = null!;
    private ImportSourceStrategyResolver _strategyResolver = null!;

    public Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        _importBatchRepository = new ImportBatchRepository(_db);
        _auditLogEntryRepository = new AuditLogEntryRepository(_db);
        _currentActorAccessor = new StubCurrentActorAccessor(new CurrentActor("manager-123", "manager@alcopilot.local", true, ["manager"]));
        _auditLogWriter = new AuditLogWriter(_auditLogEntryRepository, _currentActorAccessor);
        _tagRepository = new TagRepository(_db);
        _ingredientRepository = new IngredientRepository(_db);
        _drinkRepository = new DrinkRepository(_db);
        _drinkQueryService = new DrinkQueryService(_db);
        _workflowService = new ImportBatchProcessingService(
            _tagRepository,
            _ingredientRepository,
            _drinkQueryService);
        _applyService = new ImportBatchApplyService(
            _tagRepository,
            _ingredientRepository,
            _drinkRepository);
        _strategyResolver = new ImportSourceStrategyResolver([new IbaCocktailsSnapshotImportSourceStrategy()]);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM drink_catalog.audit_log_entries; DELETE FROM drink_catalog.\"ImportBatches\"; DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.domain_events;");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task SnapshotImportLifecycle_CreatesCatalogEntitiesAndHistory()
    {
        var createHandler = new InitializeImportBatchHandler(_strategyResolver, _importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);
        var reviewHandler = new ReviewImportBatchHandler(_importBatchRepository, _workflowService, _auditLogWriter, _db);
        var applyHandler = new ApplyImportBatchHandler(_importBatchRepository, _workflowService, _applyService, _auditLogWriter, _db);
        var historyHandler = new GetImportHistoryHandler(_importBatchRepository);

        const string payload = """
        [
          {
            "category": "Contemporary Classics",
            "name": "Daiquiri",
            "method": "Rum, lime, sugar.",
            "ingredients": [
              { "direction": "2 oz Rum", "quantity": "2", "unit": "oz", "ingredient": "Rum" },
              { "direction": "1 oz Fresh Lime Juice", "quantity": "1", "unit": "oz", "ingredient": "Fresh Lime Juice" }
            ]
          }
        ]
        """;

        var draft = await createHandler.Handle(
            new StartImportCommand(
                "iba-cocktails-snapshot",
                payload,
                new ImportSourceInput("seed/daiquiri.snapshot.json", "daiquiri.snapshot.json", "application/json", [])),
            CancellationToken.None);

        draft.Status.ShouldBe(nameof(ImportBatchStatus.InProgress));
        draft.Diagnostics.ShouldBeEmpty();
        draft.ReviewSummary.ShouldNotBeNull();
        draft.ReviewRows.Count.ShouldBeGreaterThan(0);

        var reviewed = await reviewHandler.Handle(new ReviewImportBatchCommand(draft.Id), CancellationToken.None);
        reviewed.Status.ShouldBe(nameof(ImportBatchStatus.InProgress));
        reviewed.ReviewSummary.ShouldNotBeNull();
        reviewed.ReviewSummary.CreateCount.ShouldBeGreaterThan(0);
        reviewed.RequiresReview.ShouldBeFalse();
        reviewed.ReviewRows.Count.ShouldBeGreaterThan(0);

        var applied = await applyHandler.Handle(
            new ApplyImportBatchCommand(draft.Id),
            CancellationToken.None);

        applied.WasApplied.ShouldBeTrue();
        applied.Batch.Status.ShouldBe(nameof(ImportBatchStatus.Completed));
        applied.Batch.ApplySummary.ShouldNotBeNull();
        applied.Batch.ApplySummary.CreatedCount.ShouldBeGreaterThan(0);

        var persistedBatch = await _importBatchRepository.GetByIdAsync(draft.Id, CancellationToken.None);
        persistedBatch.ShouldNotBeNull();
        persistedBatch!.Provenance.InitiatedByUserId.ShouldBe("manager-123");
        persistedBatch.Provenance.InitiatedByDisplayName.ShouldBe("manager@alcopilot.local");

        var drink = await _drinkRepository.GetByNameAsync("Daiquiri");
        drink.ShouldNotBeNull();

        var ingredient = await _ingredientRepository.GetByNameAsync("Rum");
        ingredient.ShouldNotBeNull();

        var history = await historyHandler.Handle(new GetImportHistoryQuery(), CancellationToken.None);
        history.ShouldContain(x => x.Id == draft.Id && x.Status == nameof(ImportBatchStatus.Completed));
    }

    [Fact]
    public async Task Apply_WithReviewedUpdateBatch_UpdatesExistingDrink()
    {
        var ingredient = Ingredient.Create(IngredientName.Create("Gin"), ["Old Brand"]);
        var tag = Tag.Create(TagName.Create("Classic"));
        var drink = Drink.Create(DrinkName.Create("Negroni"), DrinkCategory.Create("Contemporary Classics"), "Original", "Stir", "Orange", ImageUrl.Create(null));
        drink.SetTags([tag]);
        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient.Id, Quantity.Create("1 oz"), null)]);
        _db.Ingredients.Add(ingredient);
        _db.Tags.Add(tag);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        var createHandler = new InitializeImportBatchHandler(_strategyResolver, _importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);
        var reviewHandler = new ReviewImportBatchHandler(_importBatchRepository, _workflowService, _auditLogWriter, _db);
        var applyHandler = new ApplyImportBatchHandler(_importBatchRepository, _workflowService, _applyService, _auditLogWriter, _db);

        const string payload = """
        [
          {
            "category": "The Unforgettables",
            "name": "Negroni",
            "method": "Updated description",
            "ingredients": [
              { "direction": "1.5 oz Gin", "quantity": "1.5", "unit": "oz", "ingredient": "Gin" }
            ]
          }
        ]
        """;

        var draft = await createHandler.Handle(
            new StartImportCommand(
                "iba-cocktails-snapshot",
                payload,
                new ImportSourceInput("seed/negroni.snapshot.json", "negroni.snapshot.json", "application/json", [])),
            CancellationToken.None);

        var review = await reviewHandler.Handle(new ReviewImportBatchCommand(draft.Id), CancellationToken.None);

        review.RequiresReview.ShouldBeTrue();
        review.ReviewRows.ShouldContain(r => r.TargetType == "drink" && r.TargetKey == "Negroni" && r.RequiresReview);
        review.ReviewRows.ShouldContain(r => r.TargetType == "ingredient" && r.TargetKey == "Gin" && r.RequiresReview);

        var applied = await applyHandler.Handle(
            new ApplyImportBatchCommand(draft.Id),
            CancellationToken.None);

        applied.WasApplied.ShouldBeTrue();
        applied.Batch.Status.ShouldBe(nameof(ImportBatchStatus.Completed));
        applied.Batch.ApplySummary!.UpdatedCount.ShouldBe(2);

        var updatedDrink = await _drinkQueryService.GetDetailByIdAsync(drink.Id);
        updatedDrink.ShouldNotBeNull();
        updatedDrink!.Category.ShouldBe("The Unforgettables");
        updatedDrink.Method.ShouldBe("Updated description");
        updatedDrink.RecipeEntries.ShouldHaveSingleItem().Quantity.ShouldBe("1.5 oz");

        var updatedIngredient = await _ingredientRepository.GetByNameAsync("Gin");
        updatedIngredient.ShouldNotBeNull();
        updatedIngredient!.NotableBrands.ShouldBeEmpty();
    }

    [Fact]
    public async Task Apply_WhenBatchRequiresReviewButReviewWasNotRun_ReturnsRequiresReviewResult()
    {
        var ingredient = Ingredient.Create(IngredientName.Create("Gin"), ["Old Brand"]);
        var tag = Tag.Create(TagName.Create("Classic"));
        var drink = Drink.Create(DrinkName.Create("Negroni"), DrinkCategory.Create("Contemporary Classics"), "Original", "Stir", "Orange", ImageUrl.Create(null));
        drink.SetTags([tag]);
        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient.Id, Quantity.Create("1 oz"), null)]);
        _db.Ingredients.Add(ingredient);
        _db.Tags.Add(tag);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        var createHandler = new InitializeImportBatchHandler(_strategyResolver, _importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);
        var applyHandler = new ApplyImportBatchHandler(_importBatchRepository, _workflowService, _applyService, _auditLogWriter, _db);

        const string payload = """
        [
          {
            "category": "The Unforgettables",
            "name": "Negroni",
            "method": "Updated description",
            "ingredients": [
              { "direction": "1.5 oz Gin", "quantity": "1.5", "unit": "oz", "ingredient": "Gin" }
            ]
          }
        ]
        """;

        var draft = await createHandler.Handle(
            new StartImportCommand(
                "iba-cocktails-snapshot",
                payload,
                new ImportSourceInput("seed/negroni.snapshot.json", "negroni.snapshot.json", "application/json", [])),
            CancellationToken.None);

        var result = await applyHandler.Handle(new ApplyImportBatchCommand(draft.Id), CancellationToken.None);

        result.WasApplied.ShouldBeFalse();
        result.ApplyReadiness.ShouldBe(nameof(ImportBatchApplyReadiness.RequiresReview));
        result.Batch.Status.ShouldBe(nameof(ImportBatchStatus.InProgress));
    }

    [Fact]
    public async Task Cancel_PersistsCancelledStatusAndHistory()
    {
        var createHandler = new InitializeImportBatchHandler(_strategyResolver, _importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);
        var cancelHandler = new CancelImportBatchHandler(_importBatchRepository, _auditLogWriter, _db);
        var historyHandler = new GetImportHistoryHandler(_importBatchRepository);

        var draft = await createHandler.Handle(
            new StartImportCommand(
                "iba-cocktails-snapshot",
                string.Empty,
                new ImportSourceInput(null, null, "application/json", [])),
            CancellationToken.None);

        var cancelled = await cancelHandler.Handle(new CancelImportBatchCommand(draft.Id), CancellationToken.None);

        cancelled.Status.ShouldBe(nameof(ImportBatchStatus.Cancelled));

        var persisted = await _importBatchRepository.GetByIdAsync(draft.Id, CancellationToken.None);
        persisted.ShouldNotBeNull();
        persisted!.Status.ShouldBe(ImportBatchStatus.Cancelled);
        persisted.CancelledAtUtc.ShouldNotBeNull();

        var history = await historyHandler.Handle(new GetImportHistoryQuery(), CancellationToken.None);
        history.ShouldContain(batch => batch.Id == draft.Id && batch.Status == nameof(ImportBatchStatus.Cancelled));
    }

    private static async Task<ImportBatchDto> createAndApplyAsync(
        string payload,
        InitializeImportBatchHandler createHandler,
        ReviewImportBatchHandler reviewHandler,
        ApplyImportBatchHandler applyHandler)
    {
        var draft = await createHandler.Handle(
            new StartImportCommand(
                "iba-cocktails-snapshot",
                payload,
                new ImportSourceInput("uploads/empty.json", "empty.json", "application/json", [])),
            CancellationToken.None);

        await reviewHandler.Handle(new ReviewImportBatchCommand(draft.Id), CancellationToken.None);
        var result = await applyHandler.Handle(new ApplyImportBatchCommand(draft.Id), CancellationToken.None);
        result.WasApplied.ShouldBeTrue();
        return result.Batch;
    }

    private sealed class StubCurrentActorAccessor(CurrentActor actor) : ICurrentActorAccessor
    {
        public CurrentActor GetCurrent() => actor;
    }
}
