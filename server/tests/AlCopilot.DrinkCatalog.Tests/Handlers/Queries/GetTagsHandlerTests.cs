using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Tag;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Queries;

public sealed class GetTagsHandlerTests
{
    private readonly ITagQueryService _tagQueryService = Substitute.For<ITagQueryService>();
    private readonly GetTagsHandler _handler;

    public GetTagsHandlerTests()
    {
        _handler = new GetTagsHandler(_tagQueryService);
    }

    [Fact]
    public async Task Handle_ReturnsAllTags()
    {
        var expected = new List<TagDto> { new(Guid.NewGuid(), "Classic", 5) };
        _tagQueryService.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetTagsQuery(), CancellationToken.None);

        result.ShouldBe(expected);
    }
}
