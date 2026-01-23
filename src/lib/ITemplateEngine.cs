namespace LowlandTech.TinyTools;

/// <summary>
/// Interface for the template engine that renders templates with model data.
/// </summary>
public interface ITemplateEngine
{
    /// <summary>
    /// Renders a template string with the given context.
    /// Supports ${Context.xxx} variable interpolation and @if/@foreach control flow.
    /// </summary>
    string Render(string template, ExecutionContext context);

    /// <summary>
    /// Resolves variable expressions in a string (e.g., "${Context.Model.Name}").
    /// Used for simple interpolation without control flow.
    /// </summary>
    string ResolveVariables(string input, ExecutionContext context);
}