using AlCopilot.Shared.Domain;

namespace AlCopilot.CustomerProfile.Features.Profile;

public sealed class CustomerProfile : AggregateRoot<Guid>
{
    public string CustomerId { get; private set; } = string.Empty;
    public Guid[] FavoriteIngredientIds { get; private set; } = [];
    public Guid[] DislikedIngredientIds { get; private set; } = [];
    public Guid[] ProhibitedIngredientIds { get; private set; } = [];
    public Guid[] OwnedIngredientIds { get; private set; } = [];
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private CustomerProfile()
    {
    }

    public static CustomerProfile Create(CustomerIdentity customerId)
    {
        var now = DateTimeOffset.UtcNow;
        return new CustomerProfile
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId.Value,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
    }

    public void UpdateIngredientSets(
        IReadOnlyCollection<Guid> favoriteIngredientIds,
        IReadOnlyCollection<Guid> dislikedIngredientIds,
        IReadOnlyCollection<Guid> prohibitedIngredientIds,
        IReadOnlyCollection<Guid> ownedIngredientIds)
    {
        FavoriteIngredientIds = Normalize(favoriteIngredientIds);
        DislikedIngredientIds = Normalize(dislikedIngredientIds);
        ProhibitedIngredientIds = Normalize(prohibitedIngredientIds);
        OwnedIngredientIds = Normalize(ownedIngredientIds);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static Guid[] Normalize(IReadOnlyCollection<Guid> ingredientIds)
    {
        return ingredientIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
    }
}
