#if NET10_0_OR_GREATER
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;

/// <summary>
/// Compares the document the host serves against the committed one by the facts a consumer
/// depends on: the operation set with its identifiers, and the security schemes it declares.
/// Formatting, ordering and descriptions may change freely; an operation appearing, disappearing
/// or being renamed may not.
/// </summary>
public static class OpenApiContract
{
    public static string[] Operations(JsonElement document)
    {
        var operations = new List<string>();
        foreach (var path in document.GetProperty("paths").EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject())
            {
                var id = operation.Value.TryGetProperty("operationId", out var identifier)
                    ? identifier.GetString()
                    : "missing";
                operations.Add($"{operation.Name.ToUpperInvariant()} {path.Name} -> {id}");
            }
        }

        return operations.OrderBy(entry => entry, StringComparer.Ordinal).ToArray();
    }

    public static string[] Schemes(JsonElement document)
    {
        if (!document.TryGetProperty("components", out var components)
            || !components.TryGetProperty("securitySchemes", out var schemes)) return [];

        return schemes.EnumerateObject().Select(scheme => scheme.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray();
    }

    public static string[] Required(string[] declared, string[] security)
    {
        var missing = new List<string>();
        foreach (var operation in declared)
        {
            if (operation.EndsWith("-> missing", StringComparison.Ordinal)) missing.Add(operation);
        }

        foreach (var scheme in security)
        {
            if (!declared.Any(entry => entry.Contains(scheme, StringComparison.Ordinal))) continue;
        }

        return [.. missing];
    }

    /// <summary>The differences a consumer would notice, as readable lines; empty means no drift.</summary>
    public static string[] Differences(JsonElement served, JsonElement approved)
    {
        var differences = new List<string>();
        var servedOperations = Operations(served);
        var approvedOperations = Operations(approved);
        differences.AddRange(approvedOperations.Except(servedOperations, StringComparer.Ordinal)
            .Select(operation => "-removed " + operation));
        differences.AddRange(servedOperations.Except(approvedOperations, StringComparer.Ordinal)
            .Select(operation => "+added " + operation));
        differences.AddRange(Schemes(approved).Except(Schemes(served), StringComparer.Ordinal)
            .Select(scheme => "-scheme " + scheme));
        differences.AddRange(Schemes(served).Except(Schemes(approved), StringComparer.Ordinal)
            .Select(scheme => "+scheme " + scheme));
        return [.. differences];
    }
}
#endif
