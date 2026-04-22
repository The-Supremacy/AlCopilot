using AlCopilot.CustomerProfile.Contracts.Commands;
using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Data;
using AlCopilot.CustomerProfile.Features.Profile.Abstractions;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.CustomerProfile.Features.Profile;

public sealed class SaveCustomerProfileHandler(
    ICustomerProfileRepository customerProfileRepository,
    ICustomerProfileUnitOfWork unitOfWork,
    ICurrentActorAccessor currentActorAccessor) : IRequestHandler<SaveCustomerProfileCommand, CustomerProfileDto>
{
    public async ValueTask<CustomerProfileDto> Handle(
        SaveCustomerProfileCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = CustomerProfileActorResolver.GetCustomerId(currentActorAccessor);
        var profile = await customerProfileRepository.GetByCustomerIdAsync(customerId, cancellationToken);

        if (profile is null)
        {
            profile = CustomerProfile.Create(CustomerIdentity.Create(customerId));
            customerProfileRepository.Add(profile);
        }

        profile.UpdateIngredientSets(
            request.FavoriteIngredientIds,
            request.DislikedIngredientIds,
            request.ProhibitedIngredientIds,
            request.OwnedIngredientIds);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return profile.ToDto();
    }
}
