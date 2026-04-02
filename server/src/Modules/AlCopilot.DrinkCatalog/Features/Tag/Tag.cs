using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.Tag;

public sealed class Tag : AggregateRoot<Guid>
{
    public TagName Name { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Tag() { }

    public static Tag Create(TagName name)
    {
        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
