using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class DeleteIngredientHandlerTests
{
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteIngredientHandler _handler;

    public DeleteIngredientHandlerTests()
    {
        _handler = new DeleteIngredientHandler(_ingredientRepository, new AuditLogWriter(_auditRepository), _unitOfWork);
    }

    [Fact]
    public async Task Handle_UnreferencedIngredient_DeletesAndReturnsTrue()
    {
        var ingredient = Ingredient.Create(IngredientName.Create("Rum"));
        _ingredientRepository.GetByIdAsync(ingredient.Id, Arg.Any<CancellationToken>()).Returns(ingredient);
        _ingredientRepository.IsReferencedByActiveDrinksAsync(ingredient.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(new DeleteIngredientCommand(ingredient.Id), CancellationToken.None);

        result.ShouldBeTrue();
        _ingredientRepository.Received(1).Remove(ingredient);
    }

    [Fact]
    public async Task Handle_ReferencedIngredient_Throws()
    {
        var ingredient = Ingredient.Create(IngredientName.Create("Rum"));
        _ingredientRepository.GetByIdAsync(ingredient.Id, Arg.Any<CancellationToken>()).Returns(ingredient);
        _ingredientRepository.IsReferencedByActiveDrinksAsync(ingredient.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Should.ThrowAsync<ConflictException>(
            () => _handler.Handle(new DeleteIngredientCommand(ingredient.Id), CancellationToken.None).AsTask());
    }
}
