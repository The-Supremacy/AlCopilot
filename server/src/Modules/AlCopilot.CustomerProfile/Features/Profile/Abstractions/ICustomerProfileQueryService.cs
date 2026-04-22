using AlCopilot.CustomerProfile.Contracts.DTOs;

namespace AlCopilot.CustomerProfile.Features.Profile.Abstractions;

public interface ICustomerProfileQueryService
{
    Task<CustomerProfileDto> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default);
}
