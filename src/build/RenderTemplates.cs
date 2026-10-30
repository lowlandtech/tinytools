using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace LowlandTech.TinyTools.Build;

/// <summary>
/// Renders declared template items during the build.
///
/// Each item carries its own <c>Model</c> and <c>Output</c> metadata, so a project can render a set of
/// templates in one pass. A template that cannot render fails the build with its path and the engine
/// diagnostic, and nothing is written - the file is produced only after a complete render exists.
/// </summary>
public sealed class RenderTemplates : Task
{
    /// <summary>The template items to render.</summary>
    [Required]
    public ITaskItem[] Templates { get; set; } = Array.Empty<ITaskItem>();

    /// <inheritdoc />
    public override bool Execute()
    {
        var engine = new TinyTemplateEngine();
        var rendered = 0;

        foreach (var item in Templates)
        {
            var templatePath = item.ItemSpec;
            var output = item.GetMetadata("TinyToolsOutput");
            if (string.IsNullOrWhiteSpace(output))
            {
                Log.LogError($"Template '{templatePath}' has no TinyToolsOutput metadata.");
                return false;
            }

            if (!File.Exists(templatePath))
            {
                Log.LogError($"Template '{templatePath}' does not exist.");
                return false;
            }

            var template = File.ReadAllText(templatePath);
            var validation = engine.ValidateSyntax(template);
            if (!validation.IsValid)
            {
                var first = validation.Diagnostics[0];
                Log.LogError(
                    subcategory: "TinyTools",
                    errorCode: first.Code,
                    helpKeyword: null,
                    file: templatePath,
                    lineNumber: first.Line,
                    columnNumber: first.Column,
                    endLineNumber: first.Line,
                    endColumnNumber: first.Column,
                    message: first.Message);
                return false;
            }

            var context = new ExecutionContext();
            var modelPath = item.GetMetadata("Model");
            if (!string.IsNullOrWhiteSpace(modelPath))
            {
                if (!File.Exists(modelPath))
                {
                    Log.LogError($"Model '{modelPath}' for template '{templatePath}' does not exist.");
                    return false;
                }

                context.Model = ToModel(System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(File.ReadAllText(modelPath)));
            }

            var text = engine.Render(template, context);
            var directory = Path.GetDirectoryName(Path.GetFullPath(output));
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(output, text);
            rendered++;
            Log.LogMessage(MessageImportance.Normal, $"TinyTools rendered '{templatePath}' to '{output}'.");
        }

        Log.LogMessage(MessageImportance.High, $"TinyTools rendered {rendered} template(s).");
        return true;
    }

    /// <summary>
    /// Turns a JSON model into dictionaries, lists and primitives. Deserialising to <c>object</c>
    /// produces a <c>JsonElement</c>, which member resolution cannot read.
    /// </summary>
    private static object? ToModel(System.Text.Json.JsonElement value) => value.ValueKind switch
    {
        System.Text.Json.JsonValueKind.Object => value.EnumerateObject()
            .ToDictionary(property => property.Name, property => ToModel(property.Value), StringComparer.OrdinalIgnoreCase),
        System.Text.Json.JsonValueKind.Array => value.EnumerateArray().Select(ToModel).ToList(),
        System.Text.Json.JsonValueKind.String => value.GetString(),
        System.Text.Json.JsonValueKind.Number => value.TryGetInt64(out var integer) ? integer : value.GetDouble(),
        System.Text.Json.JsonValueKind.True => true,
        System.Text.Json.JsonValueKind.False => false,
        _ => null,
    };
}
