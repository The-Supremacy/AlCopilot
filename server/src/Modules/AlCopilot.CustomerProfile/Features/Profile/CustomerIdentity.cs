using AlCopilot.Shared.Domain;
using AlCopilot.Shared.Errors;

namespace AlCopilot.CustomerProfile.Features.Profile;

public sealed class CustomerIdentity : ValueObject<string>
{
    private CustomerIdentity(string value) : base(value)
    {
    }

    public static CustomerIdentity Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("Customer identity is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > 200)
        {
            throw new ValidationException("Customer identity must be 200 characters or fewer.");
        }

        return new CustomerIdentity(trimmed);
    }
}
