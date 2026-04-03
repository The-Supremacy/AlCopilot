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

        var domainEvent = drink.DequeueDomainEvents().ShouldHaveSingleItem();

        domainEvent.ShouldBeOfType<DrinkCreatedEvent>()
            .DrinkId.ShouldBe(drink.Id);
    }

    [Fact]
    public void DequeueDomainEvents_ClearsQueuedEvents()
    {
        var drink = Drink.Create(DrinkName.Create("Americano"), null, ImageUrl.Create(null));

        drink.DequeueDomainEvents().Count.ShouldBe(1);
        drink.DequeueDomainEvents().ShouldBeEmpty();
    }
}
