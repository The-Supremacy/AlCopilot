using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.Shared.Domain;
using Shouldly;
using Xunit;

namespace AlCopilot.DrinkCatalog.Tests.Domain;

public sealed class DomainEventTypeRegistryTests
{
    private readonly DomainEventTypeRegistry _registry = DomainEventTypeRegistry.CreateFrom(
        typeof(DrinkCreatedEvent).Assembly);

    [Fact]
    public void GetName_AppendsDefaultVersionSuffix_WhenAttributeOmitsVersion()
    {
        _registry.GetName(typeof(DrinkCreatedEvent)).ShouldBe("drink-catalog.drink-created.v1");
    }

    [Fact]
    public void GetType_ReturnsEventType_ForRegisteredLogicalName()
    {
        _registry.GetType("drink-catalog.drink-created.v1").ShouldBe(typeof(DrinkCreatedEvent));
    }

    [Fact]
    public void GetName_Throws_ForUnregisteredEventType()
    {
        Should.Throw<InvalidOperationException>(
                () => _registry.GetName(typeof(UnregisteredEvent)))
            .Message.ShouldContain("not registered");
    }

    [Fact]
    public void GetType_Throws_ForUnknownLogicalName()
    {
        Should.Throw<InvalidOperationException>(
                () => _registry.GetType("drink-catalog.unknown.v1"))
            .Message.ShouldContain("not registered");
    }

    [Fact]
    public void GetTypeNames_ContainsAllAnnotatedContractEvents()
    {
        var names = _registry.GetTypeNames();

        names[typeof(DrinkCreatedEvent)].ShouldBe("drink-catalog.drink-created.v1");
        names[typeof(DrinkDeletedEvent)].ShouldBe("drink-catalog.drink-deleted.v1");
    }

    private sealed record UnregisteredEvent(Guid AggregateId) : IDomainEvent
    {
        public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
    }
}
