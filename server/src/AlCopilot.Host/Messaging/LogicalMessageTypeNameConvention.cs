using AlCopilot.Shared.Domain;
using Rebus.Serialization;

namespace AlCopilot.Host.Messaging;

public sealed class LogicalMessageTypeNameConvention(DomainEventTypeRegistry registry) : IMessageTypeNameConvention
{
    public string GetTypeName(Type type) => registry.GetName(type);

    public Type GetType(string name) => registry.GetType(name);
}
