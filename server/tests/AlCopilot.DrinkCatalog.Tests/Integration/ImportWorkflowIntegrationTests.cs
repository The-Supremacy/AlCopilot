using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Errors;
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
    private ImportBatchWorkflowService _workflowService = null!;
    private ImportSourceStrategyResolver _strategyResolver = null!;

    public Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        _importBatchRepository = new ImportBatchRepository(_db);
        _auditLogEntryRepository = new AuditLogEntryRepository(_db);
        _currentActorAccessor = new StubCurrentActorAccessor(new CurrentActor("manager-123", "manager@alcopilot.local", true));
        _auditLogWriter = new AuditLogWriter(_auditLogEntryRepository, _currentActorAccessor);
        _tagRepository = new TagRepository(_db);
        _ingredientRepository = new IngredientRepository(_db);
        _drinkRepository = new DrinkRepository(_db);
        _drinkQueryService = new DrinkQueryService(_db);
        _workflowService = new ImportBatchWorkflowService(
            _tagRepository,
            _ingredientRepository,
            _drinkRepository,
            _drinkQueryService);
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
        var createHandler = new StartImportHandler(_strategyResolver, _importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);
        var reviewHandler = new ReviewImportBatchHandler(_importBatchRepository, _workflowService, _auditLogWriter, _db);
        var applyHandler = new ApplyImportBatchHandler(_importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);
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
        reviewed.ReviewConflicts.ShouldBeEmpty();
        reviewed.ReviewRows.Count.ShouldBeGreaterThan(0);

        var applied = await applyHandler.Handle(
            new ApplyImportBatchCommand(draft.Id, false, []),
            CancellationToken.None);

        applied.Status.ShouldBe(nameof(ImportBatchStatus.Completed));
        applied.ApplySummary.ShouldNotBeNull();
        applied.ApplySummary.CreatedCount.ShouldBeGreaterThan(0);

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
    public async Task Apply_WithConflictDecision_UpdatesExistingDrink()
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

        var createHandler = new StartImportHandler(_strategyResolver, _importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);
        var reviewHandler = new ReviewImportBatchHandler(_importBatchRepository, _workflowService, _auditLogWriter, _db);
        var applyHandler = new ApplyImportBatchHandler(_importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);

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

        review.ReviewConflicts.ShouldContain(c => c.TargetType == "drink" && c.TargetKey == "Negroni");
        review.ReviewConflicts.ShouldContain(c => c.TargetType == "ingredient" && c.TargetKey == "Gin");

        var applied = await applyHandler.Handle(
            new ApplyImportBatchCommand(
                draft.Id,
                false,
                [
                    new ImportDecisionInput("ingredient", "Gin", "approve-update", "sync brands"),
                    new ImportDecisionInput("drink", "Negroni", "approve-update", "sync recipe")
                ]),
            CancellationToken.None);

        applied.Status.ShouldBe(nameof(ImportBatchStatus.Completed));
        applied.ApplySummary!.UpdatedCount.ShouldBe(2);

        var persistedBatch = await _importBatchRepository.GetByIdAsync(draft.Id, CancellationToken.None);
        persistedBatch.ShouldNotBeNull();
        persistedBatch!.DecisionAuditTrail.ShouldAllBe(entry =>
            entry.ActorUserId == "manager-123" &&
            entry.ActorDisplayName == "manager@alcopilot.local");

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
    public async Task Apply_DuplicateFingerprintWithoutOverride_IsRejected()
    {
        var createHandler = new StartImportHandler(_strategyResolver, _importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);
        var reviewHandler = new ReviewImportBatchHandler(_importBatchRepository, _workflowService, _auditLogWriter, _db);
        var applyHandler = new ApplyImportBatchHandler(_importBatchRepository, _workflowService, _auditLogWriter, _currentActorAccessor, _db);

        const string payload = """
        [
          {
            "category": "Contemporary Classics",
            "name": "Bellini",
            "method": "Pour and stir gently.",
            "ingredients": [
              { "direction": "100 ml Prosecco", "quantity": "100", "unit": "ml", "ingredient": "Prosecco" },
              { "direction": "50 ml White Peach Puree", "quantity": "50", "unit": "ml", "ingredient": "White Peach Puree" }
            ]
          }
        ]
        """;

        var first = await createAndApplyAsync(payload, createHandler, reviewHandler, applyHandler);
        first.Status.ShouldBe(nameof(ImportBatchStatus.Completed));

        var secondDraft = await createHandler.Handle(
            new StartImportCommand(
                "iba-cocktails-snapshot",
                payload,
                new ImportSourceInput("seed/bellini.snapshot.json", "bellini.snapshot.json", "application/json", [])),
            CancellationToken.None);

        await reviewHandler.Handle(new ReviewImportBatchCommand(secondDraft.Id), CancellationToken.None);

        await Should.ThrowAsync<ConflictException>(() =>
            applyHandler.Handle(new ApplyImportBatchCommand(secondDraft.Id, false, []), CancellationToken.None).AsTask());

        var overrideApplied = await applyHandler.Handle(
            new ApplyImportBatchCommand(secondDraft.Id, true, []),
            CancellationToken.None);

        overrideApplied.Status.ShouldBe(nameof(ImportBatchStatus.Completed));
    }

    private static async Task<ImportBatchDto> createAndApplyAsync(
        string payload,
        StartImportHandler createHandler,
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
        return await applyHandler.Handle(new ApplyImportBatchCommand(draft.Id, false, []), CancellationToken.None);
    }

    private sealed class StubCurrentActorAccessor(CurrentActor actor) : ICurrentActorAccessor
    {
        public CurrentActor GetCurrent() => actor;
    }
}
