namespace AlCopilot.Shared.Domain;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DomainEventNameAttribute(string name, int version = 1) : Attribute
{
    public string Name { get; } = name;
    public int Version { get; } = version;

    public string FullName => $"{Name}.v{Version}";
}
