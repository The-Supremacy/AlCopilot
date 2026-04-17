using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class CreateIngredientHandlerTests
{
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly CreateIngredientHandler _handler;

    public CreateIngredientHandlerTests()
    {
        _handler = new CreateIngredientHandler(
            _ingredientRepository,
            new AuditLogWriter(_auditRepository),
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesIngredient()
    {
        _ingredientRepository.ExistsByNameAsync("Tequila", Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);

        var id = await _handler.Handle(
            new CreateIngredientCommand("Tequila", ["Patron"]), CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
    }
}
