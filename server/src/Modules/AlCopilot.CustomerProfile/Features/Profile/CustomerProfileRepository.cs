using AlCopilot.CustomerProfile.Features.Profile.Abstractions;
using AlCopilot.CustomerProfile.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.CustomerProfile.Features.Profile;

internal sealed class CustomerProfileRepository(CustomerProfileDbContext dbContext) : ICustomerProfileRepository
{
    public async Task<CustomerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.CustomerProfiles
            .FirstOrDefaultAsync(profile => profile.Id == id, cancellationToken);
    }

    public async Task<CustomerProfile?> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CustomerProfiles
            .FirstOrDefaultAsync(profile => profile.CustomerId == customerId, cancellationToken);
    }

    public void Add(CustomerProfile aggregate) => dbContext.CustomerProfiles.Add(aggregate);

    public void Remove(CustomerProfile aggregate) => dbContext.CustomerProfiles.Remove(aggregate);
}
