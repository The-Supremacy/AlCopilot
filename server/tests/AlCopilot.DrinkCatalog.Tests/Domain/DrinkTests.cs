using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Features.Drink;
using Shouldly;
using Xunit;

namespace AlCopilot.DrinkCatalog.Tests.Domain;

public sealed class DrinkTests
{
    [Fact]
    public void Create_RaisesDrinkCreatedDomainEvent()
    {
        var drink = Drink.Create(DrinkName.Create("Negroni"), null, ImageUrl.Create(null));

        var domainEvent = drink.DomainEvents.ShouldHaveSingleItem();

        domainEvent.ShouldBeOfType<DrinkCreatedEvent>()
            .DrinkId.ShouldBe(drink.Id);
    }

    [Fact]
    public void ClearDomainEvents_RemovesQueuedEvents()
    {
        var drink = Drink.Create(DrinkName.Create("Americano"), null, ImageUrl.Create(null));

        drink.DomainEvents.Count.ShouldBe(1);

        drink.ClearDomainEvents();

        drink.DomainEvents.ShouldBeEmpty();
    }
}
