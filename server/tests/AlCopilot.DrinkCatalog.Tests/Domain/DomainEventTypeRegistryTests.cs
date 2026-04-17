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
        names[typeof(TagCreatedEvent)].ShouldBe("drink-catalog.tag-created.v1");
        names[typeof(TagRenamedEvent)].ShouldBe("drink-catalog.tag-renamed.v1");
        names[typeof(IngredientCreatedEvent)].ShouldBe("drink-catalog.ingredient-created.v1");
        names[typeof(IngredientUpdatedEvent)].ShouldBe("drink-catalog.ingredient-updated.v1");
        names[typeof(ImportBatchInitializedEvent)].ShouldBe("drink-catalog.import-batch-initialized.v1");
        names[typeof(ImportBatchPreparedEvent)].ShouldBe("drink-catalog.import-batch-prepared.v1");
        names[typeof(ImportBatchReviewedEvent)].ShouldBe("drink-catalog.import-batch-reviewed.v1");
        names[typeof(ImportBatchCompletedEvent)].ShouldBe("drink-catalog.import-batch-completed.v1");
        names[typeof(ImportBatchCancelledEvent)].ShouldBe("drink-catalog.import-batch-cancelled.v1");
    }

    private sealed record UnregisteredEvent(Guid AggregateId) : IDomainEvent
    {
        public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
    }
}
