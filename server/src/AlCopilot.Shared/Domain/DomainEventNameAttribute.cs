namespace AlCopilot.Shared.Domain;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DomainEventNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
