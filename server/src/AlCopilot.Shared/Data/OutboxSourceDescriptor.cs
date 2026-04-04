using Microsoft.EntityFrameworkCore;

namespace AlCopilot.Shared.Data;

public sealed record OutboxSourceDescriptor(
    string Name,
    Type DbContextType)
{
    public void Validate()
    {
        if (!typeof(DbContext).IsAssignableFrom(DbContextType))
        {
            throw new InvalidOperationException(
                $"Outbox source '{Name}' DbContext type '{DbContextType.FullName}' must derive from DbContext.");
        }
    }
}
