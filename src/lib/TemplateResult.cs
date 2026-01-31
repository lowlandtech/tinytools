namespace LowlandTech.TinyTools;

/// <summary>
/// Represents the result of rendering a template.
/// Contains the generated content, file path, and namespace.
/// </summary>
public record TemplateResult
{
    /// <summary>
    /// The rendered file content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// The relative file path where this content should be written.
    /// Relative to workspace root.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// The namespace for the generated code (if applicable).
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Optional metadata for additional context.
    /// </summary>
    public Dictionary<string, object?>? Metadata { get; init; }
}
