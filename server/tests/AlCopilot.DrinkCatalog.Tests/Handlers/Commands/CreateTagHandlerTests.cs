using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class CreateTagHandlerTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly CreateTagHandler _handler;

    public CreateTagHandlerTests()
    {
        _handler = new CreateTagHandler(_tagRepository, new AuditLogWriter(_auditRepository), _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesTag()
    {
        _tagRepository.ExistsByNameAsync("Classic", Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);

        var id = await _handler.Handle(new CreateTagCommand("Classic"), CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
        _tagRepository.Received(1).Add(Arg.Any<Tag>());
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        _tagRepository.ExistsByNameAsync("Classic", Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<ConflictException>(
            () => _handler.Handle(new CreateTagCommand("Classic"), CancellationToken.None).AsTask());
    }
}
