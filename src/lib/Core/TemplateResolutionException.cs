namespace LowlandTech.TinyTools;

/// <summary>
/// Raised when a template cannot be resolved under <see cref="TemplateRenderOptions.StrictVariables"/>.
/// The code is stable so a host can react without parsing the message:
/// <c>TR001</c> unknown context key, <c>TR002</c> unknown pipe helper, <c>TR003</c> a known key
/// that resolved to null while <see cref="TemplateRenderOptions.FailOnNull"/> was set.
/// </summary>
public sealed class TemplateResolutionException(string code, string message, int line, int column)
    : InvalidOperationException(message)
{
    /// <summary>The stable diagnostic code.</summary>
    public string Code { get; } = code;

    /// <summary>One-based line of the offending expression.</summary>
    public int Line { get; } = line;

    /// <summary>One-based column of the offending expression.</summary>
    public int Column { get; } = column;
}
