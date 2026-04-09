using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class CreateTagHandlerTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateTagHandler _handler;

    public CreateTagHandlerTests()
    {
        _handler = new CreateTagHandler(_tagRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesTag()
    {
        _tagRepository.ExistsByNameAsync("Classic", Arg.Any<CancellationToken>()).Returns(false);

        var id = await _handler.Handle(new CreateTagCommand("Classic"), CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
        _tagRepository.Received(1).Add(Arg.Any<Tag>());
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        _tagRepository.ExistsByNameAsync("Classic", Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(new CreateTagCommand("Classic"), CancellationToken.None).AsTask());
    }
}
