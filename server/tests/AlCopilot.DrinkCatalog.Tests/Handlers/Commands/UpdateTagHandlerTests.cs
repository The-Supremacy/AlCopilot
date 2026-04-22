using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class UpdateTagHandlerTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly UpdateTagHandler _handler;

    public UpdateTagHandlerTests()
    {
        _handler = new UpdateTagHandler(_tagRepository, new AuditLogWriter(_auditRepository), _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenFound_RenamesTagAndReturnsTrue()
    {
        var tag = Tag.Create(TagName.Create("Refreshing"));
        _tagRepository.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _tagRepository.ExistsByNameAsync("Crisp", tag.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new UpdateTagCommand(tag.Id, "Crisp"), CancellationToken.None);

        result.ShouldBeTrue();
        tag.Name.ShouldBe(TagName.Create("Crisp"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        var tag = Tag.Create(TagName.Create("Classic"));
        _tagRepository.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _tagRepository.ExistsByNameAsync("Refreshing", tag.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<ConflictException>(
            () => _handler.Handle(new UpdateTagCommand(tag.Id, "Refreshing"), CancellationToken.None).AsTask());
    }
}
