namespace LowlandTech.TinyTools;

/// <summary>
/// Compiles a template source once so the resulting plan can be rendered repeatedly
/// against fresh contexts. Compilation never resolves variables, invokes context
/// services or captures a context.
/// </summary>
public interface ITemplateCompiler
{
    /// <summary>
    /// Compiles <paramref name="source"/> or returns the plan already cached for that
    /// exact source. Plans are cached per engine instance and may be evicted, so
    /// callers must not rely on object identity beyond a single cache window.
    /// </summary>
    CompiledTemplate Compile(string source);
}
