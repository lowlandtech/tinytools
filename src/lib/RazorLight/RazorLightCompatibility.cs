using System.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;
using LowlandTech.TinyTools;
using RazorLight.Caching;

namespace RazorLight;

/// <summary>
/// Builds a compatibility engine over the TinyTools renderer. The surface mirrors the
/// RazorLight entry points a host calls, without taking a dependency on that package.
/// </summary>
public sealed class RazorLightEngineBuilder
{
    private IRazorLightProject? _project;
    private ICachingProvider? _cache;
    private Assembly? _operatingAssembly;

    /// <summary>Reads templates from embedded resources rooted at <paramref name="rootType"/>.</summary>
    public RazorLightEngineBuilder UseEmbeddedResourcesProject(Type rootType)
    {
        if (rootType is null) throw new ArgumentNullException(nameof(rootType));
        _project = new EmbeddedResourcesProject(rootType.Assembly, rootType.Namespace);
        return this;
    }

    /// <summary>Reads templates from a filesystem root.</summary>
    public RazorLightEngineBuilder UseFileSystemProject(string root)
    {
        _project = new FileSystemProject(root);
        return this;
    }

    /// <summary>Reads templates from a filesystem root with an explicit extension.</summary>
    public RazorLightEngineBuilder UseFileSystemProject(string root, string extension)
    {
        _project = new FileSystemProject(root, extension);
        return this;
    }

    /// <summary>Sets the assembly the project resolves relative names against.</summary>
    public RazorLightEngineBuilder SetOperatingAssembly(Assembly assembly)
    {
        _operatingAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        return this;
    }

    /// <summary>Uses the in-memory caching provider.</summary>
    public RazorLightEngineBuilder UseMemoryCachingProvider()
    {
        _cache = new MemoryCachingProvider();
        return this;
    }

    /// <summary>Builds the engine; every unset part gets its documented default.</summary>
    public RazorLightEngine Build() => new(
        _project ?? new EmptyProject(),
        _cache ?? new MemoryCachingProvider(),
        _operatingAssembly ?? Assembly.GetEntryAssembly() ?? typeof(RazorLightEngineBuilder).Assembly);
}

/// <summary>
/// The compatibility engine: it compiles a template into an <see cref="ITemplatePage"/>,
/// caches it in the configured provider and renders it with the caller's model through
/// the TinyTools engine, so a cached page renders fresh model data every time.
/// </summary>
public sealed class RazorLightEngine
{
    private readonly IRazorLightProject _project;
    private readonly Assembly _operatingAssembly;

    internal RazorLightEngine(IRazorLightProject project, ICachingProvider cache, Assembly operatingAssembly)
    {
        _project = project;
        _operatingAssembly = operatingAssembly;
        Handler = new EngineHandler(cache);
    }

    /// <summary>Exposes the configured cache, mirroring the RazorLight surface.</summary>
    public EngineHandler Handler { get; }

    /// <summary>Compiles a string template, caches it under the key and renders it once.</summary>
    public Task<string> CompileRenderStringAsync<T>(string key, string content, T model, ExpandoObject? viewBag = null)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));
        Handler.Cache.CacheTemplate(key, () => new TemplatePage(content, key));
        return RenderTemplateAsync(new TemplatePage(content, key), model, viewBag);
    }

    /// <summary>Compiles the project template for the key, then renders it.</summary>
    public async Task<string> CompileRenderAsync<T>(string key, T model, ExpandoObject? viewBag = null)
    {
        var page = await CompileTemplateAsync(key).ConfigureAwait(false);
        return await RenderTemplateAsync(page, model, viewBag).ConfigureAwait(false);
    }

    /// <summary>Compiles a key into a page, reusing the cached page factory when present.</summary>
    public Task<ITemplatePage> CompileTemplateAsync(string key)
    {
        var cached = Handler.Cache.RetrieveTemplate(key);
        if (cached.Success) return Task.FromResult(cached.Template.TemplatePageFactory());

        var source = _project.GetTemplate(key, _operatingAssembly)
            ?? throw new FileNotFoundException($"Template '{key}' was not found.", key);

        Handler.Cache.CacheTemplate(key, () => new TemplatePage(source, key));
        return Task.FromResult<ITemplatePage>(new TemplatePage(source, key));
    }

    /// <summary>Renders an already compiled page against the caller's current model.</summary>
    public Task<string> RenderTemplateAsync<T>(ITemplatePage templatePage, T model, ExpandoObject? viewBag = null)
    {
        if (templatePage is null) throw new ArgumentNullException(nameof(templatePage));

        var context = new LowlandTech.TinyTools.ExecutionContext { Model = model };
        return Task.FromResult(new TinyTemplateEngine().Render(RazorSyntax.Translate(templatePage.Source), context));
    }
}

/// <summary>The engine's cache handle, mirroring the RazorLight surface.</summary>
public sealed class EngineHandler(ICachingProvider cache)
{
    /// <summary>The configured cache provider.</summary>
    public ICachingProvider Cache { get; } = cache;
}

/// <summary>Translates the Razor model syntax a compatibility template may use.</summary>
internal static class RazorSyntax
{
    private static readonly Regex ModelReference = new(@"(?<![@\w])@Model(?:(?:\.[A-Za-z_]\w*)+)", RegexOptions.Compiled);

    /// <summary>Rewrites <c>@Model.Path</c> references into TinyTools <c>${Context.Path}</c>.</summary>
    public static string Translate(string source) =>
        ModelReference.Replace(source, match => "${Context." + match.Value[1..] + "}");
}

/// <summary>A template source provider for the compatibility engine.</summary>
public interface IRazorLightProject
{
    /// <summary>Returns the template source for a key, or null when it does not exist.</summary>
    string? GetTemplate(string key, Assembly operatingAssembly);
}

/// <summary>A project with no templates, used when the builder sets none.</summary>
public sealed class EmptyProject : IRazorLightProject
{
    /// <inheritdoc />
    public string? GetTemplate(string key, Assembly operatingAssembly) => null;
}

/// <summary>Reads templates from a filesystem root and refuses paths outside it.</summary>
public sealed class FileSystemProject : IRazorLightProject
{
    private readonly string _root;
    private readonly string _extension;

    /// <summary>Uses <c>.cshtml</c> as the default extension.</summary>
    public FileSystemProject(string root) : this(root, ".cshtml")
    {
    }

    /// <summary>Uses <paramref name="extension"/> as the template extension.</summary>
    public FileSystemProject(string root, string extension)
    {
        _root = Path.GetFullPath(root ?? throw new ArgumentNullException(nameof(root)));
        _extension = string.IsNullOrWhiteSpace(extension) ? ".cshtml" : extension;
    }

    /// <inheritdoc />
    public string? GetTemplate(string key, Assembly operatingAssembly)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A template key is required.", nameof(key));

        var relative = key.EndsWith(_extension, StringComparison.OrdinalIgnoreCase) ? key : key + _extension;
        var full = Path.GetFullPath(Path.Combine(_root, relative));
        if (!full.StartsWith(_root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Template path escapes the project root: {key}");
        }

        return File.Exists(full) ? File.ReadAllText(full) : null;
    }
}

/// <summary>Reads templates embedded in an assembly under a namespace prefix.</summary>
public sealed class EmbeddedResourcesProject(Assembly assembly, string? rootNamespace) : IRazorLightProject
{
    /// <inheritdoc />
    public string? GetTemplate(string key, Assembly operatingAssembly)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A template key is required.", nameof(key));

        var prefix = string.IsNullOrWhiteSpace(rootNamespace) ? string.Empty : rootNamespace!.Replace('/', '.') + ".";
        var name = prefix + key.Replace('/', '.').Replace('\\', '.');
        if (!name.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)) name += ".cshtml";

        using var stream = assembly.GetManifestResourceStream(name);
        if (stream is null) return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
