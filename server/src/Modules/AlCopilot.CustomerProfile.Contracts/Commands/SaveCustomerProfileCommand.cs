using AlCopilot.CustomerProfile.Contracts.DTOs;
using Mediator;

namespace AlCopilot.CustomerProfile.Contracts.Commands;

public sealed record SaveCustomerProfileCommand(
    List<Guid> FavoriteIngredientIds,
    List<Guid> DislikedIngredientIds,
    List<Guid> ProhibitedIngredientIds,
    List<Guid> OwnedIngredientIds) : IRequest<CustomerProfileDto>;
