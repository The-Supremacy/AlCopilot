using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Contracts.Queries;
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
        var customerId = GetCurrentCustomerId(currentActorAccessor);
        return await queryService.GetByCustomerIdAsync(customerId, cancellationToken);
    }

    private static string GetCurrentCustomerId(ICurrentActorAccessor currentActorAccessor)
    {
        var actor = currentActorAccessor.GetCurrent();
        if (!actor.IsAuthenticated)
        {
            throw new InvalidOperationException("An authenticated customer identity is required.");
        }

        return actor.UserId
            ?? actor.DisplayName
            ?? throw new InvalidOperationException("An authenticated customer identity is required.");
    }
}
