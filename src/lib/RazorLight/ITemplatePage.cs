namespace RazorLight;

/// <summary>
/// The minimum a cached template page exposes to the compatibility engine: its cache
/// key and the template source that will be rendered. Declaring it here keeps the
/// compatibility layer free of the RazorLight package, so enabling it adds no runtime
/// dependency to the package surface.
/// </summary>
public interface ITemplatePage
{
    /// <summary>The cache key the page was compiled for.</summary>
    string Key { get; }

    /// <summary>The template source the page renders.</summary>
    string Source { get; }
}

/// <summary>A compiled template page holding its source.</summary>
public sealed class TemplatePage(string source, string key) : ITemplatePage
{
    /// <inheritdoc />
    public string Key { get; } = key;

    /// <inheritdoc />
    public string Source { get; } = source;
}
