using System.Text.Json;
using LowlandTech.TinyTools.Core.Abstractions;

namespace LowlandTech.TinyTools.Api.Services;

/// <summary>
/// Renders through this repository's own engine. The engine is stateless, so a
/// singleton keeps one instance for the process instead of one per request.
/// </summary>
public sealed class TemplateExecutor : ITemplateExecutor
{
    private readonly TinyTemplateEngine _engine = new();

    public string Render(string template, string json) => _engine.Render(template, BuildContext(json));

    public TemplateValidation Validate(string template, string json, string expectedText)
    {
        var output = Normalize(Render(template, json));
        var expected = Normalize(expectedText);
        return new TemplateValidation(string.Equals(output, expected, StringComparison.Ordinal), output, expected);
    }

    private static string Normalize(string value) => value.ReplaceLineEndings("\n").Trim();

    private static ExecutionContext BuildContext(string json)
    {
        var context = new ExecutionContext();
        using var document = JsonDocument.Parse(json);
        if (Convert(document.RootElement) is Dictionary<string, object?> values)
        {
            foreach (var (key, value) in values)
                context.Set(key, value);
        }

        return context;
    }

    private static object? Convert(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject()
            .ToDictionary(property => property.Name, property => Convert(property.Value), StringComparer.OrdinalIgnoreCase),
        JsonValueKind.Array => element.EnumerateArray().Select(Convert).ToList(),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.TryGetInt64(out var integer) ? integer : element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    };
}
