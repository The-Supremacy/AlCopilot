using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Tag;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Queries;

public sealed class GetTagsHandlerTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly GetTagsHandler _handler;

    public GetTagsHandlerTests()
    {
        _handler = new GetTagsHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_ReturnsAllTags()
    {
        var expected = new List<TagDto> { new(Guid.NewGuid(), "Classic", 5) };
        _tagRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetTagsQuery(), CancellationToken.None);

        result.ShouldBe(expected);
    }
}
