using AlCopilot.DrinkCatalog.Domain.Events;
using AlCopilot.DrinkCatalog.Domain.ValueObjects;
using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Domain.Aggregates;

public sealed class Drink : AggregateRoot<Guid>
{
    private readonly List<Tag> _tags = [];
    private readonly List<RecipeEntry> _recipeEntries = [];

    public DrinkName Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public ImageUrl ImageUrl { get; private set; } = null!;
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<RecipeEntry> RecipeEntries => _recipeEntries.AsReadOnly();

    private Drink() { }

    public static Drink Create(DrinkName name, string? description, ImageUrl imageUrl)
    {
        var drink = new Drink
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ImageUrl = imageUrl,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        drink.Raise(new DrinkCreatedEvent(drink.Id));
        return drink;
    }

    public void Update(DrinkName name, string? description, ImageUrl imageUrl)
    {
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
    }

    public void SoftDelete()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        Raise(new DrinkDeletedEvent(Id, DeletedAtUtc.Value));
    }

    public void SetTags(IEnumerable<Tag> tags)
    {
        _tags.Clear();
        _tags.AddRange(tags);
    }

    public void SetRecipeEntries(IEnumerable<RecipeEntry> entries)
    {
        _recipeEntries.Clear();
        _recipeEntries.AddRange(entries);
    }
}
