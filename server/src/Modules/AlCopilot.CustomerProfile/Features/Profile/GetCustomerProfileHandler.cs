using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.CustomerProfile.Features.Profile.Abstractions;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.CustomerProfile.Features.Profile;

public sealed class GetCustomerProfileHandler(
    ICustomerProfileQueryService queryService,
    ICurrentActorAccessor currentActorAccessor) : IRequestHandler<GetCustomerProfileQuery, CustomerProfileDto>
{
    public async ValueTask<CustomerProfileDto> Handle(
        GetCustomerProfileQuery request,
        CancellationToken cancellationToken)
    {
        var customerId = CustomerProfileActorResolver.GetCustomerId(currentActorAccessor);
        return await queryService.GetByCustomerIdAsync(customerId, cancellationToken);
    }
}
