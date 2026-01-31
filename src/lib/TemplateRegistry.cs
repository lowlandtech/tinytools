using System.Reflection;

namespace LowlandTech.TinyTools;

/// <summary>
/// Registry for managing and discovering ITemplate implementations.
/// Supports auto-discovery via reflection and manual registration.
/// </summary>
public class TemplateRegistry
{
    private readonly Dictionary<string, ITemplate> _templates = new();
    private readonly ITemplateEngine _engine;

    public TemplateRegistry(ITemplateEngine? engine = null)
    {
        _engine = engine ?? new TinyTemplateEngine();
    }

    /// <summary>
    /// Registers a template by name.
    /// </summary>
    public void Register(string name, ITemplate template)
    {
        _templates[name] = template;
    }

    /// <summary>
    /// Gets a template by name.
    /// </summary>
    public ITemplate? Get(string name)
    {
        return _templates.TryGetValue(name, out var template) ? template : null;
    }

    /// <summary>
    /// Gets all registered template names.
    /// </summary>
    public IEnumerable<string> GetNames()
    {
        return _templates.Keys;
    }

    /// <summary>
    /// Gets all registered templates.
    /// </summary>
    public IEnumerable<ITemplate> GetAll()
    {
        return _templates.Values;
    }

    /// <summary>
    /// Validates all registered templates.
    /// </summary>
    public Dictionary<string, TemplateValidationResult> ValidateAll()
    {
        var results = new Dictionary<string, TemplateValidationResult>();
        
        foreach (var (name, template) in _templates)
        {
            if (template is TemplateBase baseTemplate)
            {
                results[name] = baseTemplate.ValidateDetailed();
            }
            else
            {
                results[name] = new TemplateValidationResult
                {
                    IsValid = template.Validate(),
                    ErrorMessage = template.Validate() ? null : "Validation failed"
                };
            }
        }

        return results;
    }

    /// <summary>
    /// Auto-discovers and registers all ITemplate implementations in the specified assembly.
    /// </summary>
    public void DiscoverFromAssembly(Assembly assembly)
    {
        var templateTypes = assembly.GetTypes()
            .Where(t => typeof(ITemplate).IsAssignableFrom(t) 
                     && !t.IsInterface 
                     && !t.IsAbstract);

        foreach (var type in templateTypes)
        {
            try
            {
                var instance = Activator.CreateInstance(type) as ITemplate;
                if (instance != null)
                {
                    var name = type.Name.Replace("Template", "");
                    Register(name, instance);
                }
            }
            catch
            {
                // Skip types that can't be instantiated with default constructor
            }
        }
    }

    /// <summary>
    /// Auto-discovers and registers all ITemplate implementations in the calling assembly.
    /// </summary>
    public void DiscoverFromCallingAssembly()
    {
        var assembly = Assembly.GetCallingAssembly();
        DiscoverFromAssembly(assembly);
    }

    /// <summary>
    /// Renders a template by name with the provided data.
    /// </summary>
    public TemplateResult? Render(string name, object data)
    {
        var template = Get(name);
        return template?.Render(data);
    }

    /// <summary>
    /// Renders multiple templates in batch.
    /// </summary>
    public Dictionary<string, TemplateResult> RenderBatch(Dictionary<string, object> templateDataMap)
    {
        var results = new Dictionary<string, TemplateResult>();

        foreach (var (name, data) in templateDataMap)
        {
            var result = Render(name, data);
            if (result != null)
            {
                results[name] = result;
            }
        }

        return results;
    }
}
