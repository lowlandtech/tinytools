using System.Text.Json;

namespace LowlandTech.TinyTools;

/// <summary>
/// Interface for static file-based templates designed for large-scale code generation.
/// Templates are stored on disk and can be easily added by AI agents or developers.
/// </summary>
public interface ITemplate
{
    /// <summary>
    /// The relative path to the template file from workspace root.
    /// Example: "templates/Component.template.cs"
    /// </summary>
    string TemplatePath { get; }

    /// <summary>
    /// The namespace template for generated code.
    /// Supports variable interpolation: "MyApp.Components.${ComponentName}"
    /// </summary>
    string TemplateNamespace { get; }

    /// <summary>
    /// The template content. Can be loaded from disk or defined inline.
    /// Uses TinyTemplateEngine syntax (${...}, @if, @foreach).
    /// </summary>
    string TemplateContent { get; }

    /// <summary>
    /// The type of data expected for rendering.
    /// Defaults to Workspace. Can be any serializable type.
    /// </summary>
    Type DataType { get; }

    /// <summary>
    /// Test data as JSON string for validation.
    /// This allows templates to include their own test cases.
    /// </summary>
    string TestDataJson { get; }

    /// <summary>
    /// Expected output content when rendering with TestDataJson.
    /// Used for validation via RenderTest().
    /// </summary>
    string ExpectedContent { get; }

    /// <summary>
    /// Expected output path when rendering with TestDataJson.
    /// Optional - only validated if non-null.
    /// </summary>
    string? ExpectedPath { get; }

    /// <summary>
    /// Expected output namespace when rendering with TestDataJson.
    /// Optional - only validated if non-null.
    /// </summary>
    string? ExpectedNamespace { get; }

    /// <summary>
    /// Renders the template with the provided data.
    /// </summary>
    /// <param name="data">The data object to render. Will be serialized/deserialized to match DataType.</param>
    /// <returns>A TemplateResult containing content, path, and namespace.</returns>
    TemplateResult Render(object data);

    /// <summary>
    /// Validates the template by rendering with TestDataJson and comparing against expected outputs.
    /// </summary>
    /// <returns>True if rendered output matches all expected values; false otherwise.</returns>
    bool Validate();
}

/// <summary>
/// Validation result with detailed information about what failed.
/// </summary>
public record TemplateValidationResult
{
    public required bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public TemplateResult? ActualResult { get; init; }
    public Dictionary<string, (string Expected, string Actual)>? Differences { get; init; }
    public string? DetailedDiff { get; init; } // Character-level diff for content

    public static TemplateValidationResult Success(TemplateResult result) =>
        new() { IsValid = true, ActualResult = result };

    public static TemplateValidationResult Failure(string error, TemplateResult? actual = null) =>
        new() { IsValid = false, ErrorMessage = error, ActualResult = actual };

    public static TemplateValidationResult Mismatch(
        TemplateResult actual,
        Dictionary<string, (string Expected, string Actual)> differences) =>
        new()
        {
            IsValid = false,
            ErrorMessage = $"Validation failed with {differences.Count} mismatch(es)",
            ActualResult = actual,
            Differences = differences,
            DetailedDiff = BuildDetailedDiff(differences)
        };

    /// <summary>
    /// Builds a detailed character-level diff showing exactly what's different.
    /// </summary>
    private static string? BuildDetailedDiff(Dictionary<string, (string Expected, string Actual)> differences)
    {
        if (!differences.ContainsKey("Content"))
            return null;

        var (expected, actual) = differences["Content"];
        var diff = new System.Text.StringBuilder();
        
        diff.AppendLine("===== CONTENT DIFF =====");
        diff.AppendLine($"Expected Length: {expected.Length}");
        diff.AppendLine($"Actual Length: {actual.Length}");
        diff.AppendLine();

        // Find first difference
        int firstDiff = -1;
        for (int i = 0; i < Math.Min(expected.Length, actual.Length); i++)
        {
            if (expected[i] != actual[i])
            {
                firstDiff = i;
                break;
            }
        }

        if (firstDiff >= 0)
        {
            diff.AppendLine($"First difference at position {firstDiff}:");
            diff.AppendLine($"  Expected char: '{EscapeChar(expected[firstDiff])}' (code: {(int)expected[firstDiff]})");
            diff.AppendLine($"  Actual char: '{EscapeChar(actual[firstDiff])}' (code: {(int)actual[firstDiff]})");
            
            // Show context around the difference
            var start = Math.Max(0, firstDiff - 40);
            var length = Math.Min(80, expected.Length - start);
            diff.AppendLine();
            diff.AppendLine("Expected context:");
            diff.AppendLine($"  ...{EscapeString(expected.Substring(start, length))}...");
            
            if (firstDiff < actual.Length)
            {
                start = Math.Max(0, firstDiff - 40);
                length = Math.Min(80, actual.Length - start);
                diff.AppendLine("Actual context:");
                diff.AppendLine($"  ...{EscapeString(actual.Substring(start, length))}...");
            }
        }
        else if (expected.Length != actual.Length)
        {
            diff.AppendLine($"Lengths differ: expected {expected.Length}, got {actual.Length}");
            if (expected.Length < actual.Length)
            {
                diff.AppendLine($"Extra {actual.Length - expected.Length} characters in actual");
            }
            else
            {
                diff.AppendLine($"Missing {expected.Length - actual.Length} characters in actual");
            }
        }

        diff.AppendLine();
        diff.AppendLine("===== FULL EXPECTED =====");
        diff.AppendLine(expected);
        diff.AppendLine();
        diff.AppendLine("===== FULL ACTUAL =====");
        diff.AppendLine(actual);
        diff.AppendLine("===== END DIFF =====");

        return diff.ToString();
    }

    private static string EscapeChar(char c) => c switch
    {
        '\n' => "\\n",
        '\r' => "\\r",
        '\t' => "\\t",
        ' ' => "·", // visible space
        _ => c.ToString()
    };

    private static string EscapeString(string s) =>
        s.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
}

