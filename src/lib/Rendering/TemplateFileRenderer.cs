namespace LowlandTech.TinyTools.Rendering;

/// <summary>The outcome of rendering one template input.</summary>
public sealed record TemplateRenderResult(string Input, bool Success, string Message, string? OutputPath = null);

/// <summary>
/// Renders template files against JSON models and writes the output only after a complete
/// render exists, so a failure can never leave a partial file behind.
///
/// The contract the CLI exposes:
/// <list type="bullet">
/// <item>Every output path must stay inside the workspace root; anything else is a named refusal.</item>
/// <item>A malformed template or model fails with the input named and the existing output untouched.</item>
/// <item>A batch reports one result per input in the order given and does not stop at the first
/// failure, so a caller sees every problem in one run.</item>
/// </list>
/// </summary>
public sealed class TemplateFileRenderer(TinyTemplateEngine? engine = null)
{
    private readonly TinyTemplateEngine _engine = engine ?? new TinyTemplateEngine();

    /// <summary>Renders one template file to <paramref name="outputPath"/>.</summary>
    public TemplateRenderResult Render(
        string templatePath,
        string? modelPath,
        string outputPath,
        string root)
    {
        if (string.IsNullOrWhiteSpace(templatePath)) throw new ArgumentException("A template path is required.", nameof(templatePath));
        if (string.IsNullOrWhiteSpace(outputPath)) throw new ArgumentException("An output path is required.", nameof(outputPath));

        var fullOutput = ResolveInsideRoot(outputPath, root);

        string template;
        try
        {
            template = File.ReadAllText(templatePath);
        }
        catch (IOException error)
        {
            return new(templatePath, false, $"Could not read template '{templatePath}': {error.Message}");
        }

        object? model = null;
        if (!string.IsNullOrWhiteSpace(modelPath))
        {
            try
            {
                model = ToModel(System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(File.ReadAllText(modelPath)));
            }
            catch (System.Text.Json.JsonException error)
            {
                return new(templatePath, false, $"Model '{modelPath}' is not valid JSON: {error.Message}");
            }
            catch (IOException error)
            {
                return new(templatePath, false, $"Could not read model '{modelPath}': {error.Message}");
            }
        }

        string rendered;
        try
        {
            var context = new ExecutionContext { Model = model };
            rendered = _engine.Render(template, context);
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException)
        {
            // Named refusal: the input is identified and the previous output is untouched
            // because the file is only written after a complete render exists.
            return new(templatePath, false, $"Template '{templatePath}' failed: {error.Message}");
        }

        var directory = Path.GetDirectoryName(fullOutput);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(fullOutput, rendered);

        return new(templatePath, true, $"Rendered '{templatePath}' to '{outputPath}'.", outputPath);
    }

    /// <summary>
    /// Renders every input in order. A failing entry does not stop the batch; the caller
    /// decides the exit code from the results.
    /// </summary>
    public IReadOnlyList<TemplateRenderResult> RenderBatch(
        IEnumerable<(string Template, string? Model, string Output)> inputs,
        string root) =>
        inputs.Select(input => Render(input.Template, input.Model, input.Output, root)).ToList();

    private static object? ToModel(System.Text.Json.JsonElement value) => value.ValueKind switch
    {
        System.Text.Json.JsonValueKind.Object => value.EnumerateObject().ToDictionary(
            property => property.Name, property => ToModel(property.Value), StringComparer.OrdinalIgnoreCase),
        System.Text.Json.JsonValueKind.Array => value.EnumerateArray().Select(ToModel).ToList(),
        System.Text.Json.JsonValueKind.String => value.GetString(),
        System.Text.Json.JsonValueKind.Number => value.TryGetInt64(out var integer) ? integer : value.GetDouble(),
        System.Text.Json.JsonValueKind.True => true,
        System.Text.Json.JsonValueKind.False => false,
        _ => null,
    };

    /// <summary>Resolves an output path and refuses anything outside the workspace root.</summary>
    public static string ResolveInsideRoot(string path, string root)
    {
        var fullRoot = Path.GetFullPath(root);
        var fullPath = Path.GetFullPath(Path.Combine(fullRoot, path));
        var inside = string.Equals(fullPath, fullRoot, StringComparison.OrdinalIgnoreCase)
                     || fullPath.StartsWith(fullRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        if (!inside)
        {
            throw new InvalidOperationException($"Output path escapes the workspace root: {path}");
        }

        return fullPath;
    }
}
