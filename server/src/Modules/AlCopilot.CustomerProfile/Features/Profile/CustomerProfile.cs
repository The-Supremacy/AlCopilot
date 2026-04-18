using AlCopilot.CustomerProfile.Contracts.Events;
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
        var profile = new CustomerProfile
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId.Value,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        profile.Raise(new CustomerProfileCreatedEvent(profile.Id, profile.CustomerId));
        return profile;
    }

    public void UpdateIngredientSets(
        IReadOnlyCollection<Guid> favoriteIngredientIds,
        IReadOnlyCollection<Guid> dislikedIngredientIds,
        IReadOnlyCollection<Guid> prohibitedIngredientIds,
        IReadOnlyCollection<Guid> ownedIngredientIds)
    {
        var normalizedFavoriteIngredientIds = Normalize(favoriteIngredientIds);
        var normalizedDislikedIngredientIds = Normalize(dislikedIngredientIds);
        var normalizedProhibitedIngredientIds = Normalize(prohibitedIngredientIds);
        var normalizedOwnedIngredientIds = Normalize(ownedIngredientIds);

        if (FavoriteIngredientIds.SequenceEqual(normalizedFavoriteIngredientIds) &&
            DislikedIngredientIds.SequenceEqual(normalizedDislikedIngredientIds) &&
            ProhibitedIngredientIds.SequenceEqual(normalizedProhibitedIngredientIds) &&
            OwnedIngredientIds.SequenceEqual(normalizedOwnedIngredientIds))
        {
            return;
        }

        FavoriteIngredientIds = normalizedFavoriteIngredientIds;
        DislikedIngredientIds = normalizedDislikedIngredientIds;
        ProhibitedIngredientIds = normalizedProhibitedIngredientIds;
        OwnedIngredientIds = normalizedOwnedIngredientIds;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        Raise(new CustomerProfileUpdatedEvent(Id, CustomerId));
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
