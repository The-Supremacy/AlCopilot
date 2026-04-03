using System.Reflection;

namespace AlCopilot.Shared.Domain;

public sealed class DomainEventTypeRegistry
{
    private readonly IReadOnlyDictionary<Type, string> _typeToName;
    private readonly IReadOnlyDictionary<string, Type> _nameToType;

    private DomainEventTypeRegistry(
        IReadOnlyDictionary<Type, string> typeToName,
        IReadOnlyDictionary<string, Type> nameToType)
    {
        _typeToName = typeToName;
        _nameToType = nameToType;
    }

    public static DomainEventTypeRegistry CreateFrom(Assembly assembly)
    {
        var typeToName = new Dictionary<Type, string>();
        var nameToType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || !typeof(IDomainEvent).IsAssignableFrom(type))
            {
                continue;
            }

            var attribute = type.GetCustomAttribute<DomainEventNameAttribute>();
            if (attribute is null || string.IsNullOrWhiteSpace(attribute.Name))
            {
                continue;
            }

            var logicalName = attribute.Name.EndsWith(".v1", StringComparison.OrdinalIgnoreCase)
                ? attribute.Name
                : $"{attribute.Name}.v1";

            typeToName[type] = logicalName;
            nameToType[logicalName] = type;
        }

        return new DomainEventTypeRegistry(typeToName, nameToType);
    }

    public string GetName(Type eventType)
    {
        if (_typeToName.TryGetValue(eventType, out var name))
        {
            return name;
        }

        throw new InvalidOperationException(
            $"Domain event type '{eventType.FullName}' is not registered.");
    }

    public Type GetType(string logicalName)
    {
        if (_nameToType.TryGetValue(logicalName, out var eventType))
        {
            return eventType;
        }

        throw new InvalidOperationException($"Domain event logical name '{logicalName}' is not registered.");
    }

    public IReadOnlyDictionary<Type, string> GetTypeNames() => _typeToName;
}
