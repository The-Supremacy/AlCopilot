using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Drink;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Queries;

public sealed class GetDrinkByIdHandlerTests
{
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly GetDrinkByIdHandler _handler;

    public GetDrinkByIdHandlerTests()
    {
        _handler = new GetDrinkByIdHandler(_drinkRepository);
    }

    [Fact]
    public async Task Handle_WhenFound_ReturnsDetail()
    {
        var id = Guid.NewGuid();
        var expected = new DrinkDetailDto(id, "Test", null, null, null, null, null, [], []);
        _drinkRepository.GetDetailByIdAsync(id, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _handler.Handle(new GetDrinkByIdQuery(id), CancellationToken.None);

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNull()
    {
        _drinkRepository.GetDetailByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DrinkDetailDto?)null);

        var result = await _handler.Handle(new GetDrinkByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}
