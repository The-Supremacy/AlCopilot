using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AlCopilot.DrinkCatalog.Data.Configurations;

internal static class JsonValueComparerFactory
{
    public static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web);

    public static ValueComparer<T> Create<T>()
    {
        return new ValueComparer<T>(
            (left, right) => Serialize(left) == Serialize(right),
            value => Serialize(value).GetHashCode(),
            value => Deserialize<T>(Serialize(value)));
    }

    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, SerializerOptions);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, SerializerOptions)
            ?? throw new InvalidOperationException($"Unable to deserialize JSON into {typeof(T).Name}.");
    }
}
