using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class CancelImportBatchHandlerTests
{
    private readonly IImportBatchRepository _importBatchRepository = Substitute.For<IImportBatchRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly CancelImportBatchHandler _handler;

    public CancelImportBatchHandlerTests()
    {
        _handler = new CancelImportBatchHandler(
            _importBatchRepository,
            new AuditLogWriter(_auditRepository),
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenBatchIsActive_MarksBatchForUpdateAndPersists()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        _importBatchRepository.GetByIdAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);

        var result = await _handler.Handle(new CancelImportBatchCommand(batch.Id), CancellationToken.None);

        result.Status.ShouldBe(nameof(ImportBatchStatus.Cancelled));
        _importBatchRepository.Received(1).Update(batch);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
