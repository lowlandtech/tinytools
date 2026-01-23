using System.Reflection;
using System.Text.RegularExpressions;
using RegexMatch = System.Text.RegularExpressions.Match;

namespace LowlandTech.TinyTools;

/// <summary>
/// Resolves ${Context.xxx} variable expressions.
/// </summary>
public partial class VariableResolver
{
    [GeneratedRegex(@"\$\{([^}]+)\}")]
    private static partial Regex VariablePattern();

    [GeneratedRegex(@"\s*\|\s*(\w+)(?::(.+?))?(?=\s*\||$)")]
    private static partial Regex PipePattern();

    /// <summary>
    /// Resolves all ${Context.xxx} expressions in a string.
    /// Supports null coalescing with ?? operator: ${Context.Name ?? "Default"}
    /// Supports pipe helpers: ${Context.Name | upper | truncate:20}
    /// </summary>
    public string ResolveString(string input, ExecutionContext context)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return VariablePattern().Replace(input, match =>
        {
            var expression = match.Groups[1].Value;
            
            // Handle null coalescing operator (??) - check before pipes
            if (expression.Contains("??") && !expression.Contains('|'))
            {
                var parts = expression.Split("??", 2);
                var value = ResolveExpressionWithPipes(parts[0].Trim(), context);
                if (value == null || (value is string s && string.IsNullOrEmpty(s)))
                {
                    // Return the default value (trim quotes if present)
                    var defaultValue = parts[1].Trim().Trim('"', '\'');
                    return defaultValue;
                }
                return value.ToString() ?? string.Empty;
            }
            
            var result = ResolveExpressionWithPipes(expression, context);
            return result?.ToString() ?? string.Empty;
        });
    }

    /// <summary>
    /// Resolves an expression that may contain pipe helpers.
    /// </summary>
    private object? ResolveExpressionWithPipes(string expression, ExecutionContext context)
    {
        // Check for pipe operators
        var pipeIndex = expression.IndexOf('|');
        if (pipeIndex == -1)
        {
            return ResolveExpression(expression.Trim(), context);
        }

        // Split expression and helpers
        var variableExpr = expression[..pipeIndex].Trim();
        var helpersExpr = expression[pipeIndex..];

        // Resolve the base value
        var value = ResolveExpression(variableExpr, context);

        // Apply each helper in sequence
        var matches = PipePattern().Matches(helpersExpr);
        foreach (RegexMatch helperMatch in matches)
        {
            var helperName = helperMatch.Groups[1].Value;
            var helperArg = helperMatch.Groups[2].Success ? helperMatch.Groups[2].Value : null;
            value = TemplateHelpers.Apply(value, helperName, helperArg);
        }

        return value;
    }

    /// <summary>
    /// Resolves a variable expression and returns the typed value.
    /// </summary>
    public object? ResolveExpression(string expression, ExecutionContext context)
    {
        // Handle Context.Get("key") syntax
        if (expression.StartsWith("Context.Get(", StringComparison.OrdinalIgnoreCase))
        {
            var keyMatch = Regex.Match(expression, @"Context\.Get\([""']([^""']+)[""']\)");
            if (keyMatch.Success)
            {
                return context.Get(keyMatch.Groups[1].Value);
            }
        }

        // Handle Context.xxx.yyy path syntax
        if (expression.StartsWith("Context.", StringComparison.OrdinalIgnoreCase))
        {
            var path = expression.Substring("Context.".Length);
            return ResolvePath(path, context);
        }

        // Direct path resolution
        return ResolvePath(expression, context);
    }

    /// <summary>
    /// Resolves a property path on the context.
    /// Supports method calls with string arguments: Services('key')
    /// Supports chained calls: Services('key')('value')
    /// </summary>
    private object? ResolvePath(string path, ExecutionContext context)
    {
        // Check if path contains method calls
        if (path.Contains('('))
        {
            return ResolvePathWithMethodCalls(path, context);
        }

        var parts = path.Split('.');
        if (parts.Length == 0) return null;

        // First part is the context key
        var currentValue = context.Get(parts[0]);
        if (currentValue == null) return null;

        // Navigate remaining path
        for (int i = 1; i < parts.Length; i++)
        {
            currentValue = GetPropertyValue(currentValue, parts[i]);
            if (currentValue == null) return null;
        }

        return currentValue;
    }

    /// <summary>
    /// Resolves a path that contains method calls with arguments.
    /// Examples: Services('pluralize')('word'), Services('calc')(Context.Value)
    /// </summary>
    private object? ResolvePathWithMethodCalls(string path, ExecutionContext context)
    {
        // Start with context as current value
        object? currentValue = context;
        var index = 0;

        // Pattern to match method call: methodName('arg') or methodName("arg")
        var methodCallPattern = new Regex(@"^(\w+)\('([^']*)'\)|^(\w+)\(""([^""]*)""\)");
        // Pattern to match direct invocation with string literal: ('arg') or ("arg")
        var directCallLiteralPattern = new Regex(@"^\('([^']*)'\)|^\(""([^""]*)""\)");
        // Pattern to match direct invocation with variable: (varName) or (Context.Path.To.Value)
        var directCallVariablePattern = new Regex(@"^\(([^)]+)\)");

        while (index < path.Length)
        {
            var remaining = path.Substring(index);
            
            // Try matching a method call first (methodName('arg'))
            var methodMatch = methodCallPattern.Match(remaining);
            if (methodMatch.Success)
            {
                var methodName = !string.IsNullOrEmpty(methodMatch.Groups[1].Value) ? methodMatch.Groups[1].Value : methodMatch.Groups[3].Value;
                var argument = !string.IsNullOrEmpty(methodMatch.Groups[2].Value) ? methodMatch.Groups[2].Value : methodMatch.Groups[4].Value;

                currentValue = InvokeMethodOrProperty(currentValue, methodName, argument, context);
                if (currentValue == null) return null;

                index += methodMatch.Length;
                continue;
            }
            
            // Try matching a direct invocation with string literal (('arg'))
            var directLiteralMatch = directCallLiteralPattern.Match(remaining);
            if (directLiteralMatch.Success)
            {
                var argument = !string.IsNullOrEmpty(directLiteralMatch.Groups[1].Value) ? directLiteralMatch.Groups[1].Value : directLiteralMatch.Groups[2].Value;
                
                currentValue = InvokeDelegate(currentValue, argument, context);
                if (currentValue == null) return null;
                
                index += directLiteralMatch.Length;
                continue;
            }
            
            // Try matching a direct invocation with variable ((Context.EntityName))
            var directVariableMatch = directCallVariablePattern.Match(remaining);
            if (directVariableMatch.Success)
            {
                var variableExpression = directVariableMatch.Groups[1].Value.Trim();
                
                // Resolve the variable to get its value
                var argumentValue = ResolveExpression(variableExpression, context);
                
                currentValue = InvokeDelegate(currentValue, argumentValue, context);
                if (currentValue == null) return null;
                
                index += directVariableMatch.Length;
                continue;
            }
            
            // No more method calls matched - handle remaining path
            remaining = remaining.TrimStart('.');
            if (!string.IsNullOrEmpty(remaining))
            {
                // If remaining contains '(' and the '(' is NOT at the start, 
                // we need to navigate properties up to the method call
                var parenIndex = remaining.IndexOf('(');
                if (parenIndex > 0)
                {
                    var propertyPath = remaining.Substring(0, parenIndex);
                    
                    // Navigate the property path (e.g., "Object.MyDelegate")
                    var propertyParts = propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in propertyParts)
                    {
                        currentValue = GetPropertyValue(currentValue!, part);
                        if (currentValue == null) return null;
                    }
                    
                    // Update index to continue processing from the method call
                    index += parenIndex;
                    continue;
                }
                else if (parenIndex == -1)
                {
                    // No method calls - just navigate properties
                    foreach (var part in remaining.Split('.', StringSplitOptions.RemoveEmptyEntries))
                    {
                        currentValue = GetPropertyValue(currentValue!, part);
                        if (currentValue == null) return null;
                    }
                }
                // else parenIndex == 0, which means we have something like "('test')"
                // This should have been caught by the patterns above, so we break
            }
            break;
        }

        return currentValue;
    }

    /// <summary>
    /// Invokes a method or property on an object with a string argument.
    /// </summary>
    private object? InvokeMethodOrProperty(object? currentValue, string methodName, string argument, ExecutionContext context)
    {
        if (currentValue == null) return null;

        var type = currentValue.GetType();
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        
        if (method != null)
        {
            try
            {
                return method.Invoke(currentValue, new object[] { argument });
            }
            catch
            {
                return null;
            }
        }
        
        // Try as property/field that might be a delegate
        var member = GetPropertyOrMethodValue(currentValue, methodName);
        
        if (member is Delegate del)
        {
            try
            {
                return del.DynamicInvoke(argument);
            }
            catch
            {
                return null;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Invokes a delegate with an argument (which can be a value or another object).
    /// </summary>
    private object? InvokeDelegate(object? currentValue, object? argument, ExecutionContext context)
    {
        if (currentValue is Delegate del)
        {
            try
            {
                return del.DynamicInvoke(argument);
            }
            catch
            {
                return null;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Gets a property or method value from an object.
    /// </summary>
    private static object? GetPropertyOrMethodValue(object obj, string name)
    {
        if (obj == null) return null;

        var type = obj.GetType();

        // Try property first
        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property != null)
        {
            return property.GetValue(obj);
        }

        // Try field
        var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (field != null)
        {
            return field.GetValue(obj);
        }

        return null;
    }

    /// <summary>
    /// Gets a property value from an object by name.
    /// </summary>
    private static object? GetPropertyValue(object obj, string propertyName)
    {
        if (obj == null) return null;

        // Special handling for ExecutionContext - use Get() method
        if (obj is ExecutionContext ctx)
        {
            return ctx.Get(propertyName);
        }

        var type = obj.GetType();

        // Try property
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property != null)
        {
            return property.GetValue(obj);
        }

        // Try field
        var field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (field != null)
        {
            return field.GetValue(obj);
        }

        // Try dictionary
        if (obj is IDictionary<string, object?> dict)
        {
            return dict.TryGetValue(propertyName, out var value) ? value : null;
        }

        if (obj is IDictionary<object, object?> objDict)
        {
            return objDict.TryGetValue(propertyName, out var value) ? value : null;
        }

        return null;
    }

    /// <summary>
    /// Resolves inputs dictionary, replacing any ${Context.xxx} expressions.
    /// </summary>
    public Dictionary<string, object?> ResolveInputs(Dictionary<string, object?> inputs, ExecutionContext context)
    {
        var resolved = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in inputs)
        {
            resolved[kvp.Key] = ResolveValue(kvp.Value, context);
        }

        return resolved;
    }

    /// <summary>
    /// Resolves a single value, handling strings, lists, and dictionaries.
    /// </summary>
    public object? ResolveValue(object? value, ExecutionContext context)
    {
        return value switch
        {
            null => null,
            // For "${Context.xxx}", extract "Context.xxx" (skip 2 chars for "${", trim 1 char for "}")
            string str when str.StartsWith("${") && str.EndsWith("}") =>
                ResolveExpression(str[2..^1], context),
            string str => ResolveString(str, context),
            List<object> list => list.Select(item => ResolveValue(item, context)).ToList(),
            Dictionary<string, object?> dict => ResolveInputs(dict, context),
            Dictionary<object, object?> objDict => objDict.ToDictionary(
                kvp => kvp.Key.ToString() ?? string.Empty,
                kvp => ResolveValue(kvp.Value, context)),
            _ => value
        };
    }
}
