using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Features.Drink;
using Shouldly;
using Xunit;

namespace AlCopilot.DrinkCatalog.UnitTests.Domain;

public sealed class DrinkTests
{
    [Fact]
    public void Create_RaisesDrinkCreatedDomainEvent()
    {
        var drink = Drink.Create(DrinkName.Create("Negroni"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));

        var domainEvent = drink.DomainEvents.ShouldHaveSingleItem();

        domainEvent.ShouldBeOfType<DrinkCreatedEvent>()
            .DrinkId.ShouldBe(drink.Id);
    }

    [Fact]
    public void ClearDomainEvents_RemovesQueuedEvents()
    {
        var drink = Drink.Create(DrinkName.Create("Americano"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));

        drink.DomainEvents.Count.ShouldBe(1);

        drink.ClearDomainEvents();

        drink.DomainEvents.ShouldBeEmpty();
    }
}
