using System.Collections.Frozen;
using System.Reflection;

namespace AlCopilot.Shared.Domain;

public sealed class DomainEventTypeRegistry
{
    private readonly FrozenDictionary<Type, string> _typeToName;
    private readonly FrozenDictionary<string, Type> _nameToType;

    private DomainEventTypeRegistry(
        FrozenDictionary<Type, string> typeToName,
        FrozenDictionary<string, Type> nameToType)
    {
        _typeToName = typeToName;
        _nameToType = nameToType;
    }

    public string GetName(Type eventType) =>
        _typeToName.TryGetValue(eventType, out var name)
            ? name
            : throw new InvalidOperationException(
                $"Domain event type '{eventType.FullName}' is not registered. " +
                $"Add [DomainEventName] attribute to the event class.");

    public Type GetType(string eventName) =>
        _nameToType.TryGetValue(eventName, out var type)
            ? type
            : throw new InvalidOperationException(
                $"Domain event name '{eventName}' is not registered. " +
                $"No event class with [DomainEventName(\"{eventName}\")] was found.");

    public IReadOnlyDictionary<Type, string> GetTypeNames() => _typeToName;

    public static DomainEventTypeRegistry CreateFrom(params Assembly[] assemblies)
    {
        var typeToName = new Dictionary<Type, string>();
        var nameToType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<DomainEventNameAttribute>();
                if (attribute is null)
                {
                    continue;
                }

                if (!typeof(IDomainEvent).IsAssignableFrom(type))
                {
                    throw new InvalidOperationException(
                        $"Type '{type.FullName}' has [DomainEventName] but does not implement IDomainEvent.");
                }

                var fullName = attribute.FullName;

                if (nameToType.TryGetValue(fullName, out var existing))
                {
                    throw new InvalidOperationException(
                        $"Duplicate domain event name '{fullName}' on types " +
                        $"'{existing.FullName}' and '{type.FullName}'.");
                }

                typeToName[type] = fullName;
                nameToType[fullName] = type;
            }
        }

        return new DomainEventTypeRegistry(
            typeToName.ToFrozenDictionary(),
            nameToType.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase));
    }
}
