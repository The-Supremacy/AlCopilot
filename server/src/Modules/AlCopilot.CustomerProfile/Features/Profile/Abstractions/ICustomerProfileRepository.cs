namespace AlCopilot.CustomerProfile.Features.Profile.Abstractions;

public interface ICustomerProfileRepository
{
    Task<CustomerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CustomerProfile?> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken = default);

    void Add(CustomerProfile aggregate);

    void Remove(CustomerProfile aggregate);
}
