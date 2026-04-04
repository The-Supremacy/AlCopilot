using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.Host.Messaging;
using AlCopilot.Shared.Domain;
using Shouldly;
using Xunit;

namespace AlCopilot.Host.Tests;

public sealed class LogicalConventionTests
{
    private readonly DomainEventTypeRegistry _registry = DomainEventTypeRegistry.CreateFrom(
        typeof(DrinkCreatedEvent).Assembly);

    [Fact]
    public void MessageTypeConvention_UsesLogicalEventName()
    {
        var convention = new LogicalMessageTypeNameConvention(_registry);

        convention.GetTypeName(typeof(DrinkCreatedEvent)).ShouldBe("drink-catalog.drink-created.v1");
    }

    [Fact]
    public void TopicConvention_UsesLogicalEventName()
    {
        var convention = new LogicalTopicNameConvention(_registry);

        convention.GetTopic(typeof(DrinkCreatedEvent)).ShouldBe("drink-catalog.drink-created.v1");
    }
}
