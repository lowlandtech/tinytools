namespace LowlandTech.TinyTools;

/// <summary>
/// Runtime context for pipeline execution.
/// Contains all variables accessible via ${Context.xxx} expressions.
/// </summary>
public class ExecutionContext
{
    private readonly Dictionary<string, object?> _variables = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<CursorEntry> _cursorStack = new();
    private readonly Dictionary<string, TemplateServiceFunc> _services = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Unique key for this context (used by ContextFactory).
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Parent context (if this is a child context).
    /// </summary>
    public ExecutionContext? Parent { get; private set; }

    /// <summary>
    /// Registers a template service function with a key.
    /// For simple inline services.
    /// Usage: context.RegisterService("upper", input => input?.ToString()?.ToUpper())
    /// </summary>
    public void RegisterService(string key, TemplateServiceFunc service)
    {
        _services[key] = service;
    }

    /// <summary>
    /// Registers a template service instance (IoC/DI scenario).
    /// The service's Name property determines the key.
    /// Usage: context.RegisterService(new HumanizerService())
    /// </summary>
    public void RegisterService(ITemplateService service)
    {
        _services[service.Name] = service.Transform;
    }

    /// <summary>
    /// Registers multiple template services from an IoC container.
    /// Useful when services are resolved from dependency injection.
    /// Usage: context.RegisterServices(serviceProvider.GetServices&lt;ITemplateService&gt;())
    /// </summary>
    public void RegisterServices(IEnumerable<ITemplateService> services)
    {
        foreach (var service in services)
        {
            RegisterService(service);
        }
    }

    /// <summary>
    /// Resolves a template service by key.
    /// Returns a function that can be called with input.
    /// If service not found, returns a function that returns "{key} not registered".
    /// Usage: ${Context.Services("pluralize")("word")}
    /// </summary>
    public TemplateServiceFunc Services(string key)
    {
        if (_services.TryGetValue(key, out var service))
        {
            return service;
        }

        // Service not found - return error function
        return _ => $"{{{key} not registered}}";
    }

    /// <summary>
    /// The root model passed to the pipeline (accessible as ${Context.Model}).
    /// </summary>
    public object? Model
    {
        get => Get("Model");
        set => Set("Model", value);
    }

    /// <summary>
    /// The output path for generated files (accessible as ${Context.OutputPath}).
    /// </summary>
    public string? OutputPath
    {
        get => Get("OutputPath") as string;
        set => Set("OutputPath", value);
    }

    /// <summary>
    /// The current object being processed (accessible as ${Context.Current}).
    /// This is the top of the cursor stack.
    /// </summary>
    public object? Current => _cursorStack.TryPeek(out var entry) ? entry.Item : null;

    /// <summary>
    /// The current index when iterating (accessible as ${Context.CurrentIndex}).
    /// </summary>
    public int CurrentIndex => _cursorStack.TryPeek(out var entry) ? entry.Index : -1;

    /// <summary>
    /// The current key when iterating (accessible as ${Context.CurrentKey}).
    /// </summary>
    public string? CurrentKey => _cursorStack.TryPeek(out var entry) ? entry.Key : null;

    /// <summary>
    /// Pushes a new item onto the cursor stack (used during iteration).
    /// </summary>
    public void PushCursor(object? item, int index = 0, string? key = null)
    {
        _cursorStack.Push(new CursorEntry(item, index, key));
        // Also expose as variable for template access
        Set("Current", item);
        Set("CurrentIndex", index);
        Set("CurrentKey", key);
    }

    /// <summary>
    /// Pops the current item from the cursor stack.
    /// </summary>
    public void PopCursor()
    {
        if (_cursorStack.Count > 0)
        {
            _cursorStack.Pop();
            // Update variables to reflect new top of stack
            if (_cursorStack.TryPeek(out var entry))
            {
                Set("Current", entry.Item);
                Set("CurrentIndex", entry.Index);
                Set("CurrentKey", entry.Key);
            }
            else
            {
                Set("Current", null);
                Set("CurrentIndex", -1);
                Set("CurrentKey", null);
            }
        }
    }

    /// <summary>
    /// Gets the cursor stack depth.
    /// </summary>
    public int CursorDepth => _cursorStack.Count;

    private record CursorEntry(object? Item, int Index, string? Key);

    /// <summary>
    /// Gets a variable by key.
    /// </summary>
    public object? Get(string key)
    {
        return _variables.TryGetValue(key, out var value) ? value : null;
    }


    /// <summary>
    /// Sets a variable by key.
    /// </summary>
    public void Set(string key, object? value)
    {
        _variables[key] = value;
    }

    /// <summary>
    /// Checks if a variable exists.
    /// </summary>
    public bool Has(string key)
    {
        return _variables.ContainsKey(key);
    }

    /// <summary>
    /// Gets all variable keys.
    /// </summary>
    public IEnumerable<string> Keys => _variables.Keys;

    /// <summary>
    /// Creates a child context that inherits from this context.
    /// Used for nested pipeline execution (e.g., foreach loops).
    /// </summary>
    public ExecutionContext CreateChild(string? key = null)
    {
        var child = new ExecutionContext
        {
            Key = key,
            Parent = this
        };
        foreach (var kvp in _variables)
        {
            child._variables[kvp.Key] = kvp.Value;
        }
        foreach (var kvp in _services)
        {
            child._services[kvp.Key] = kvp.Value;
        }
        return child;
    }

    /// <summary>
    /// Merges values from another context (typically used to propagate child outputs to parent).
    /// </summary>
    public void Merge(ExecutionContext other, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (other.Has(key))
            {
                Set(key, other.Get(key));
            }
        }
    }
}
