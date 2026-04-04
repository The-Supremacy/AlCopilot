namespace AlCopilot.Shared.Domain;

public abstract class ValueObject<T> : IEquatable<ValueObject<T>>
{
    public T Value { get; }

    protected ValueObject(T value)
    {
        Value = value;
    }

    public static implicit operator T(ValueObject<T> valueObject) => valueObject.Value;

    public override bool Equals(object? obj) =>
        obj is ValueObject<T> other && Equals(other);

    public bool Equals(ValueObject<T>? other) =>
        other is not null && EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override int GetHashCode() =>
        Value is null ? 0 : EqualityComparer<T>.Default.GetHashCode(Value);

    public override string ToString() => Value?.ToString() ?? string.Empty;

    public static bool operator ==(ValueObject<T>? left, ValueObject<T>? right) =>
        Equals(left, right);

    public static bool operator !=(ValueObject<T>? left, ValueObject<T>? right) =>
        !Equals(left, right);
}
