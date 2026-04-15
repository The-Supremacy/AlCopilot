using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.CustomerProfile.Features.Profile;

internal sealed class CustomerProfileQueryService(CustomerProfileDbContext dbContext) : ICustomerProfileQueryService
{
    public async Task<CustomerProfileDto> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.CustomerProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(profile => profile.CustomerId == customerId, cancellationToken);

        return profile is null ? CustomerProfileMappings.Empty() : profile.ToDto();
    }
}
