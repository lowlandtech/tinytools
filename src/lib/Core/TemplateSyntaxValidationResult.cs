namespace LowlandTech.TinyTools;

/// <summary>
/// The outcome of validating a template's syntax before rendering. This is a
/// parse/validate contract only: it never renders, never invokes helpers and says
/// nothing about whether a render would match an expected output. That comparison
/// stays with <see cref="TemplateValidationResult"/>.
/// </summary>
/// <param name="IsValid">Whether the template's syntax is supported.</param>
/// <param name="Diagnostics">The stable diagnostics for an invalid template; empty when valid.</param>
public sealed record TemplateSyntaxValidationResult(bool IsValid, IReadOnlyList<TemplateSyntaxDiagnostic> Diagnostics)
{
    /// <summary>A valid result with no diagnostics.</summary>
    public static TemplateSyntaxValidationResult Valid { get; } = new(true, []);

    /// <summary>The diagnostic codes of an invalid result, in source order.</summary>
    public IEnumerable<string> Codes => Diagnostics.Select(diagnostic => diagnostic.Code);
}

/// <summary>A single, stable syntax diagnostic. Coordinates are owned by a later scope.</summary>
/// <param name="Code">A stable code such as <c>TS001</c>.</param>
/// <param name="Message">A stable human-readable description.</param>
public sealed record TemplateSyntaxDiagnostic(string Code, string Message);
