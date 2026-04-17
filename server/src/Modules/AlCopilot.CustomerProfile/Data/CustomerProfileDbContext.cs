using AlCopilot.CustomerProfile.Features.Profile;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.CustomerProfile.Data;

public sealed class CustomerProfileDbContext(DbContextOptions<CustomerProfileDbContext> options)
    : DbContext(options), ICustomerProfileUnitOfWork
{
    public DbSet<Features.Profile.CustomerProfile> CustomerProfiles => Set<Features.Profile.CustomerProfile>();
    public DbSet<DomainEventRecord> DomainEventRecords => Set<DomainEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customer_profile");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerProfileDbContext).Assembly);
    }
}
