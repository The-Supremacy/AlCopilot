using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class CreateIngredientCategoryHandlerTests
{
    private readonly IIngredientCategoryRepository _repository = Substitute.For<IIngredientCategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateIngredientCategoryHandler _handler;

    public CreateIngredientCategoryHandlerTests()
    {
        _handler = new CreateIngredientCategoryHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesCategory()
    {
        _repository.ExistsByNameAsync("Spirits", Arg.Any<CancellationToken>()).Returns(false);

        var id = await _handler.Handle(new CreateIngredientCategoryCommand("Spirits"), CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_DuplicateName_Throws()
    {
        _repository.ExistsByNameAsync("Spirits", Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(new CreateIngredientCategoryCommand("Spirits"), CancellationToken.None).AsTask());
    }
}
