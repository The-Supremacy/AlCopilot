using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class Drink
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private Drink()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public List<RecipeEntry> RecipeEntries { get; private set; } = [];
    public List<AlCopilot.DrinkCatalog.Features.Tag.Tag> Tags { get; private set; } = [];

    public static Drink Create(DrinkName name, string? description, ImageUrl imageUrl)
    {
        var drink = new Drink
        {
            Id = Guid.NewGuid(),
            Name = name.Value,
            Description = description,
            ImageUrl = imageUrl.Value,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        drink._domainEvents.Add(new DrinkCreatedEvent(drink.Id));
        return drink;
    }

    public IReadOnlyList<IDomainEvent> DequeueDomainEvents()
    {
        var events = _domainEvents.ToArray();
        _domainEvents.Clear();
        return events;
    }
}
