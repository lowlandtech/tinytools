namespace LowlandTech.TinyTools.Core.Abstractions;

/// <summary>
/// Renders templates through the engine this repository publishes, so the
/// server cannot drift from the package it ships.
/// </summary>
public interface ITemplateExecutor
{
    /// <summary>Renders a template with a JSON model.</summary>
    string Render(string template, string json);

    /// <summary>Renders a template and compares the output with the expected text.</summary>
    TemplateValidation Validate(string template, string json, string expectedText);
}
