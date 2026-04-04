using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.Shared.Domain;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Domain;

public sealed class AggregateRootTests
{
    [Fact]
    public void AggregateRoot_RaisesEvent_CollectedInDomainEvents()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));

        drink.DomainEvents.Count.ShouldBe(1);
        drink.DomainEvents[0].ShouldBeOfType<DrinkCreatedEvent>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));
        drink.ClearDomainEvents();

        drink.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void SoftDelete_AddsSecondEvent()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));
        drink.SoftDelete();

        drink.DomainEvents.Count.ShouldBe(2);
        drink.DomainEvents[0].ShouldBeOfType<DrinkCreatedEvent>();
        drink.DomainEvents[1].ShouldBeOfType<DrinkDeletedEvent>();
    }

    [Fact]
    public void DomainEvent_HasCorrectAggregateId()
    {
        var drink = Drink.Create(DrinkName.Create("Test"), null, ImageUrl.Create(null));
        var evt = (DrinkCreatedEvent)drink.DomainEvents[0];

        evt.AggregateId.ShouldBe(drink.Id);
        evt.OccurredAtUtc.ShouldNotBe(default);
    }
}
