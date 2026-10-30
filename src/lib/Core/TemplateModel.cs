using System.Text.Json;

namespace LowlandTech.TinyTools;

/// <summary>
/// Turns JSON into the shape variable resolution expects: dictionaries, lists and primitives.
///
/// Deserialising to <c>object</c> yields a <c>JsonElement</c>, which member access cannot read, so
/// every surface that accepts a JSON model - the file renderer, the MSBuild task, the CLI and the
/// playground - converts through here instead of each carrying its own copy.
/// </summary>
public static class TemplateModel
{
    /// <summary>Converts a JSON document into dictionaries, lists and primitives.</summary>
    public static object? FromJson(string json)
    {
        try
        {
            return FromElement(JsonSerializer.Deserialize<JsonElement>(json));
        }
        catch (JsonException)
        {
            throw;
        }
    }

    /// <summary>Converts one JSON element into dictionaries, lists and primitives.</summary>
    public static object? FromElement(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Object => value.EnumerateObject()
            .ToDictionary(property => property.Name, property => FromElement(property.Value), StringComparer.OrdinalIgnoreCase),
        JsonValueKind.Array => value.EnumerateArray().Select(FromElement).ToList(),
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.TryGetInt64(out var integer) ? integer : value.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    };
}
