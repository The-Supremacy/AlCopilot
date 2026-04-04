using AlCopilot.Shared.Domain;
using Rebus.Topic;

namespace AlCopilot.Host.Messaging;

public sealed class LogicalTopicNameConvention(DomainEventTypeRegistry registry) : ITopicNameConvention
{
    public string GetTopic(Type eventType) => registry.GetName(eventType);
}
