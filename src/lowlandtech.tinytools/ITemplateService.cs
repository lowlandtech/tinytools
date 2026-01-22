namespace LowlandTech.TinyTools;

/// <summary>
/// Interface for template services that transform input data.
/// Implement this interface for IoC/DI scenarios.
/// For simple use cases, use TemplateServiceFunc delegate directly.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// The key used to access this service in templates.
    /// Example: "pluralize", "calc", "format"
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Transforms the input and returns the result.
    /// </summary>
    /// <param name="input">The input value to transform.</param>
    /// <returns>The transformed value.</returns>
    object? Transform(object? input);
}

/// <summary>
/// Delegate for simple template service functions.
/// Use this for inline registrations without creating a class.
/// </summary>
public delegate object? TemplateServiceFunc(object? input);


