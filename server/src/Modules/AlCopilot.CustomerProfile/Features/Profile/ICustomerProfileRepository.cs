using AlCopilot.Shared.Data;

namespace AlCopilot.CustomerProfile.Features.Profile;

public interface ICustomerProfileRepository : IRepository<CustomerProfile, Guid>
{
    Task<CustomerProfile?> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default);
}
