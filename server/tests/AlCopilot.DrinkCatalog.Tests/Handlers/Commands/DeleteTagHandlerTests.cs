using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class DeleteTagHandlerTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly DeleteTagHandler _handler;

    public DeleteTagHandlerTests()
    {
        _handler = new DeleteTagHandler(_tagRepository, new AuditLogWriter(_auditRepository), _unitOfWork);
    }

    [Fact]
    public async Task Handle_UnreferencedTag_DeletesAndReturnsTrue()
    {
        var tag = Tag.Create(TagName.Create("Old"));
        _tagRepository.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _tagRepository.IsReferencedByDrinksAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new DeleteTagCommand(tag.Id), CancellationToken.None);

        result.ShouldBeTrue();
        _tagRepository.Received(1).Remove(tag);
    }

    [Fact]
    public async Task Handle_ReferencedTag_Throws()
    {
        var tag = Tag.Create(TagName.Create("Active"));
        _tagRepository.GetByIdAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(tag);
        _tagRepository.IsReferencedByDrinksAsync(tag.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<ConflictException>(
            () => _handler.Handle(new DeleteTagCommand(tag.Id), CancellationToken.None).AsTask());
    }
}
