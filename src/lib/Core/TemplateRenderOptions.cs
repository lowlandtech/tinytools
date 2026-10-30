namespace LowlandTech.TinyTools;

/// <summary>
/// How a render treats unresolved values. The default is lenient, which is what every existing
/// template and the whole test suite already assume: an unknown key renders as an empty string.
/// Strict mode is opt-in because a template that generated acceptable output yesterday must not
/// start throwing today.
/// </summary>
public sealed record TemplateRenderOptions
{
    /// <summary>Refuse an expression whose context key does not exist.</summary>
    public bool StrictVariables { get; init; }

    /// <summary>
    /// Refuse an expression whose key exists but resolves to null. Only meaningful with
    /// <see cref="StrictVariables"/>, because the key has to exist before it can be null.
    /// </summary>
    public bool FailOnNull { get; init; }

    /// <summary>The behavior every existing caller gets: unknown keys and nulls render empty.</summary>
    public static TemplateRenderOptions Lenient { get; } = new();

    /// <summary>Unknown keys and nulls are both refusals.</summary>
    public static TemplateRenderOptions Strict { get; } = new() { StrictVariables = true, FailOnNull = true };
}
