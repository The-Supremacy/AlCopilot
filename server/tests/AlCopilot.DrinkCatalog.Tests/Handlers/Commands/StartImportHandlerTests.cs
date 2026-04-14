using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class StartImportHandlerTests
{
    private readonly IImportSourceStrategyResolver _strategyResolver = Substitute.For<IImportSourceStrategyResolver>();
    private readonly IImportSourceStrategy _strategy = Substitute.For<IImportSourceStrategy>();
    private readonly IImportBatchRepository _repository = Substitute.For<IImportBatchRepository>();
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly IDrinkQueryService _drinkQueryService = Substitute.For<IDrinkQueryService>();
    private readonly ImportBatchWorkflowService _workflowService;
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly ICurrentActorAccessor _currentActorAccessor = Substitute.For<ICurrentActorAccessor>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly StartImportHandler _handler;

    public StartImportHandlerTests()
    {
        _workflowService = new ImportBatchWorkflowService(
            Substitute.For<AlCopilot.DrinkCatalog.Features.Tag.ITagRepository>(),
            Substitute.For<AlCopilot.DrinkCatalog.Features.Ingredient.IIngredientRepository>(),
            _drinkRepository,
            _drinkQueryService);
        _currentActorAccessor.GetCurrent().Returns(new CurrentActor("user-123", "manager@alcopilot.local", true));
        _handler = new StartImportHandler(_strategyResolver, _repository, _workflowService, new AuditLogWriter(_auditRepository, _currentActorAccessor), _currentActorAccessor, _unitOfWork);
    }

    [Fact]
    public async Task Handle_CreatesBatchWithValidationAndReviewSnapshot()
    {
        _strategy.Key.Returns(ImportStrategyKey.IbaCocktailsSnapshot);
        _strategyResolver.GetRequired("iba-cocktails-snapshot").Returns(_strategy);
        _drinkQueryService.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);
        _strategy.CreateImportAsync(Arg.Any<ImportSourceStrategyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ImportSourceStrategyResult(
                "sha256:test",
                new ImportProvenance("uploads/catalog.json", "catalog.json", "application/json", []),
                new NormalizedCatalogImport([new NormalizedTagImport("Classic")], [], []),
                []));

        var result = await _handler.Handle(
            new StartImportCommand(
                "iba-cocktails-snapshot",
                "{}",
                new ImportSourceInput("uploads/catalog.json", "catalog.json", "application/json", [])),
            CancellationToken.None);

        result.StrategyKey.ShouldBe("iba-cocktails-snapshot");
        result.Status.ShouldBe(nameof(ImportBatchStatus.InProgress));
        result.SourceFingerprint.ShouldBe("sha256:test");
        result.ReviewSummary.ShouldNotBeNull();
        result.ReviewSummary.CreateCount.ShouldBe(1);
        result.ReviewConflicts.ShouldBeEmpty();
        _repository.Received(1).Add(Arg.Is<ImportBatch>(batch =>
            batch.Provenance.InitiatedByUserId == "user-123" &&
            batch.Provenance.InitiatedByDisplayName == "manager@alcopilot.local"));
        _auditRepository.Received().Add(Arg.Is<AuditLogEntry>(entry =>
            entry.ActorUserId == "user-123" &&
            entry.Actor == "manager@alcopilot.local"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
