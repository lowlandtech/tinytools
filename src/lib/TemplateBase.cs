using System.Text.Json;

namespace LowlandTech.TinyTools;

/// <summary>
/// Base class for implementing ITemplate with common functionality.
/// Handles rendering, validation, and data serialization.
/// </summary>
public abstract class TemplateBase : ITemplate
{
    private readonly ITemplateEngine _engine;

    protected TemplateBase(ITemplateEngine? engine = null)
    {
        _engine = engine ?? new TinyTemplateEngine();
    }

    public abstract string TemplatePath { get; }
    public abstract string TemplateNamespace { get; }
    public abstract string TemplateContent { get; }
    
    public virtual Type DataType => typeof(object);
    
    public virtual string TestDataJson => "{}";
    public virtual string ExpectedContent => string.Empty;
    public virtual string? ExpectedPath => null;
    public virtual string? ExpectedNamespace => null;

    public virtual TemplateResult Render(object data)
    {
        // Serialize and deserialize to ensure data matches expected type
        var normalizedData = NormalizeData(data);
        
        // Create execution context and populate with data properties
        var context = new ExecutionContext();
        PopulateContext(context, normalizedData);
        
        // Render content
        var content = _engine.Render(TemplateContent, context);
        
        // Render path (path template may contain variables)
        var path = _engine.Render(TemplatePath, context);
        
        // Render namespace (namespace template may contain variables)
        var ns = _engine.Render(TemplateNamespace, context);
        
        return new TemplateResult
        {
            Content = content,
            Path = path,
            Namespace = ns
        };
    }

    /// <summary>
    /// Populates the execution context with all properties from the data object.
    /// This allows ${Context.PropertyName} syntax to work correctly.
    /// </summary>
    private static void PopulateContext(ExecutionContext context, object data)
    {
        var type = data.GetType();
        foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            var value = prop.GetValue(data);
            context.Set(prop.Name, value);
        }
    }

    public virtual bool Validate()
    {
        var result = ValidateDetailed();
        return result.IsValid;
    }

    /// <summary>
    /// Validates the template and returns detailed results.
    /// </summary>
    public virtual TemplateValidationResult ValidateDetailed()
    {
        try
        {
            // Deserialize test data
            var testData = JsonSerializer.Deserialize(TestDataJson, DataType);
            if (testData == null)
            {
                return TemplateValidationResult.Failure("Failed to deserialize TestDataJson");
            }

            // Render with test data
            var actual = Render(testData);
            
            // Compare results (normalize and trim for consistent cross-platform comparison)
            var differences = new Dictionary<string, (string Expected, string Actual)>();
            
            var normalizedExpected = NormalizeAndTrim(ExpectedContent);
            var normalizedActual = NormalizeAndTrim(actual.Content);
            
            if (!string.IsNullOrEmpty(ExpectedContent) && normalizedActual != normalizedExpected)
            {
                differences["Content"] = (ExpectedContent, actual.Content);
            }
            
            if (ExpectedPath != null && actual.Path != ExpectedPath)
            {
                differences["Path"] = (ExpectedPath, actual.Path);
            }
            
            if (ExpectedNamespace != null && actual.Namespace != ExpectedNamespace)
            {
                differences["Namespace"] = (ExpectedNamespace, actual.Namespace);
            }

            if (differences.Count > 0)
            {
                return TemplateValidationResult.Mismatch(actual, differences);
            }

            return TemplateValidationResult.Success(actual);
        }
        catch (Exception ex)
        {
            return TemplateValidationResult.Failure($"Validation threw exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Normalizes line endings and trims whitespace for consistent comparison.
    /// </summary>
    private static string NormalizeAndTrim(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input ?? string.Empty;
        return input.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }

    /// <summary>
    /// Normalizes data by serializing and deserializing to the expected DataType.
    /// This ensures consistency regardless of input type.
    /// </summary>
    protected virtual object NormalizeData(object data)
    {
        // If data is already the correct type, return as-is
        if (data.GetType() == DataType)
        {
            return data;
        }

        // Serialize to JSON and deserialize to target type
        var json = JsonSerializer.Serialize(data);
        var normalized = JsonSerializer.Deserialize(json, DataType);
        
        return normalized ?? throw new InvalidOperationException(
            $"Failed to normalize data to type {DataType.Name}");
    }
}
