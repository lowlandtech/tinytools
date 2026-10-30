namespace LowlandTech.TinyTools;

/// <summary>
/// An immutable compiled template plan. It keeps the original source and renders it
/// against whatever context the caller supplies, so it never stores a context, a
/// model, a service or rendered output.
/// </summary>
public sealed class CompiledTemplate
{
    private readonly Func<ToolContext, string> _render;

    internal CompiledTemplate(string source, string normalizedSource, Func<ToolContext, string> render, ITemplateEngine engine)
    {
        Source = source;
        NormalizedSource = normalizedSource;
        _render = render;
        Engine = engine;
    }

    /// <summary>The exact source string this plan was compiled from.</summary>
    public string Source { get; }

    /// <summary>The source with line endings normalized, as the plan renders it.</summary>
    public string NormalizedSource { get; }

    /// <summary>The engine that owns this plan.</summary>
    public ITemplateEngine Engine { get; }

    /// <summary>Renders the plan against the supplied current context.</summary>
    public string Render(ToolContext context) => _render(context);
}
