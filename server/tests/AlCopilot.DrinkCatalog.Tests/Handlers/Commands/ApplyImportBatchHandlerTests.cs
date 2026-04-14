using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using AlCopilot.Shared.Models;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class ApplyImportBatchHandlerTests
{
    private readonly IImportBatchRepository _importBatchRepository = Substitute.For<IImportBatchRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly IDrinkQueryService _drinkQueryService = Substitute.For<IDrinkQueryService>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly ICurrentActorAccessor _currentActorAccessor = Substitute.For<ICurrentActorAccessor>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ApplyImportBatchHandler _handler;

    public ApplyImportBatchHandlerTests()
    {
        var workflowService = new ImportBatchWorkflowService(
            _tagRepository,
            _ingredientRepository,
            _drinkRepository,
            _drinkQueryService);
        _currentActorAccessor.GetCurrent().Returns(new CurrentActor("user-123", "manager@alcopilot.local", true));
        _handler = new ApplyImportBatchHandler(
            _importBatchRepository,
            workflowService,
            new AuditLogWriter(_auditRepository, _currentActorAccessor),
            _currentActorAccessor,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenDuplicateFingerprintExistsWithoutOverride_Throws()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []),
            "sha256:same");
        batch.RecordValidation([]);

        var existingApplied = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []),
            "sha256:same");
        existingApplied.MarkCompleted(new ImportApplySummary(0, 0, 0, 0));

        _importBatchRepository.GetByIdAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        _importBatchRepository.GetAppliedByStrategyAndFingerprintAsync(ImportStrategyKey.IbaCocktailsSnapshot, "sha256:same", Arg.Any<CancellationToken>())
            .Returns(existingApplied);

        await Should.ThrowAsync<ConflictException>(
            () => _handler.Handle(
                new ApplyImportBatchCommand(batch.Id, false, []),
                CancellationToken.None).AsTask());
    }
}
