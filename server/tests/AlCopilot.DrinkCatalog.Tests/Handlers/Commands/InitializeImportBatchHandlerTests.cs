using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class InitializeImportBatchHandlerTests
{
    private readonly IImportSourceStrategyResolver _strategyResolver = Substitute.For<IImportSourceStrategyResolver>();
    private readonly IImportSourceStrategy _strategy = Substitute.For<IImportSourceStrategy>();
    private readonly IImportBatchRepository _repository = Substitute.For<IImportBatchRepository>();
    private readonly IImportBatchProcessingService _processingService = Substitute.For<IImportBatchProcessingService>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly ICurrentActorAccessor _currentActorAccessor = Substitute.For<ICurrentActorAccessor>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly InitializeImportBatchHandler _handler;

    public InitializeImportBatchHandlerTests()
    {
        _currentActorAccessor.GetCurrent().Returns(new CurrentActor("user-123", "manager@alcopilot.local", true, ["manager"]));
        _handler = new InitializeImportBatchHandler(_strategyResolver, _repository, _processingService, new AuditLogWriter(_auditRepository, _currentActorAccessor), _currentActorAccessor, _unitOfWork);
    }

    [Fact]
    public async Task Handle_CreatesBatchWithValidationAndReviewSnapshot()
    {
        _strategy.Key.Returns(ImportStrategyKey.IbaCocktailsSnapshot);
        _strategyResolver.GetRequired("iba-cocktails-snapshot").Returns(_strategy);
        _strategy.CreateImportAsync(Arg.Any<ImportSourceStrategyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ImportSourceStrategyResult(
                new ImportProvenance("uploads/catalog.json", "catalog.json", "application/json", []),
                new NormalizedCatalogImport([new NormalizedTagImport("Classic")], [], []),
                []));
        _processingService.ProcessAsync(Arg.Any<NormalizedCatalogImport>(), Arg.Any<CancellationToken>())
            .Returns(new ImportBatchProcessingResult(
                [],
                new ImportReviewSummary(1, 0, 0),
                [new ImportReviewRow("tag", "Classic", "create", "Create tag 'Classic'.", false, false)]));

        var result = await _handler.Handle(
            new StartImportCommand(
                "iba-cocktails-snapshot",
                "{}",
                new ImportSourceInput("uploads/catalog.json", "catalog.json", "application/json", [])),
            CancellationToken.None);

        result.StrategyKey.ShouldBe("iba-cocktails-snapshot");
        result.Status.ShouldBe(nameof(ImportBatchStatus.InProgress));
        result.ApplyReadiness.ShouldBe(nameof(ImportBatchApplyReadiness.Ready));
        result.ReviewSummary.ShouldNotBeNull();
        result.ReviewSummary.CreateCount.ShouldBe(1);
        result.RequiresReview.ShouldBeFalse();
        result.ReviewedAtUtc.ShouldBeNull();
        _repository.Received(1).Add(Arg.Is<ImportBatch>(batch =>
            batch.Provenance.InitiatedByUserId == "user-123" &&
            batch.Provenance.InitiatedByDisplayName == "manager@alcopilot.local"));
        _auditRepository.Received().Add(Arg.Is<AuditLogEntry>(entry =>
            entry.ActorUserId == "user-123" &&
            entry.Actor == "manager@alcopilot.local"));
        await _processingService.Received(1).ProcessAsync(Arg.Any<NormalizedCatalogImport>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
