using AlCopilot.CustomerProfile.Contracts.DTOs;

namespace AlCopilot.CustomerProfile.Features.Profile;

internal static class CustomerProfileMappings
{
    public static CustomerProfileDto ToDto(this CustomerProfile profile)
    {
        return new CustomerProfileDto(
            profile.FavoriteIngredientIds.ToList(),
            profile.DislikedIngredientIds.ToList(),
            profile.ProhibitedIngredientIds.ToList(),
            profile.OwnedIngredientIds.ToList());
    }

    public static CustomerProfileDto Empty()
    {
        return new CustomerProfileDto([], [], [], []);
    }
}
