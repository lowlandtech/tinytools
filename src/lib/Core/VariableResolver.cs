namespace LowlandTech.TinyTools;

/// <summary>
/// Resolves ${Context.xxx} variable expressions.
/// </summary>
public partial class VariableResolver
{
    private readonly ILogger _logger;

    public VariableResolver(ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.CreateLogger<VariableResolver>()
                  ?? NullLogger<VariableResolver>.Instance;
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"\$\{([^}]+)\}")]
    private static partial Regex VariablePattern();
#else
    private static readonly Regex _variablePattern = new Regex(@"\$\{([^}]+)\}", RegexOptions.Compiled);
    private static Regex VariablePattern() => _variablePattern;
#endif

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"\s*\|\s*(\w+)(?::(.+?))?(?=\s*\||$)")]
    private static partial Regex PipePattern();
#else
    private static readonly Regex _pipePattern = new Regex(@"\s*\|\s*(\w+)(?::(.+?))?(?=\s*\||$)", RegexOptions.Compiled);
    private static Regex PipePattern() => _pipePattern;
#endif

    /// <summary>
    /// Resolves all ${Context.xxx} expressions in a string.
    /// Supports null coalescing with ?? operator: ${Context.Name ?? "Default"}
    /// Supports pipe helpers: ${Context.Name | upper | truncate:20}
    /// </summary>
    public string ResolveString(string input, ToolContext context)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return VariablePattern().Replace(input, match =>
        {
            var expression = match.Groups[1].Value;
            
            // Handle ternary operator (condition ? trueValue : falseValue)
            if (IsTernaryExpression(expression))
            {
                return EvaluateTernary(expression, context);
            }
            
            // Handle null coalescing operator (??) - check before pipes
            if (expression.Contains("??") && !expression.Contains('|'))
            {
                var parts = expression.Split("??", 2);
                var value = ResolveExpressionWithPipes(parts[0].Trim(), context);
                if (value == null || (value is string s && string.IsNullOrEmpty(s)))
                {
                    var defaultExpr = parts[1].Trim();
                    _logger.LogDebug("Null coalescing: '{Expression}' was null/empty, using default '{Default}'",
                        parts[0].Trim(), defaultExpr);
                    // Quoted literal — strip quotes and return as-is
                    if ((defaultExpr.StartsWith('"') && defaultExpr.EndsWith('"')) ||
                        (defaultExpr.StartsWith('\'') && defaultExpr.EndsWith('\'')))
                    {
                        return defaultExpr.Trim('"', '\'');
                    }
                    // Dotted path — resolve as expression
                    if (defaultExpr.Contains('.'))
                    {
                        var resolved = ResolveExpressionWithPipes(defaultExpr, context);
                        return resolved?.ToString() ?? string.Empty;
                    }
                    // Unquoted bare word — treat as literal string
                    return defaultExpr;
                }
                return value.ToString() ?? string.Empty;
            }

            var result = ResolveExpressionWithPipes(expression, context);
            var resultStr = result?.ToString() ?? string.Empty;
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("${{{Expression}}} → \"{Result}\"", expression,
                    resultStr.Length > 100 ? resultStr[..100] + "..." : resultStr);
            }
            return resultStr;
        });
    }

    /// <summary>
    /// Checks if an expression is a ternary expression.
    /// </summary>
    private static bool IsTernaryExpression(string expression)
    {
        // Must contain ? and : but not be a method call or null coalescing
        if (!expression.Contains('?') || !expression.Contains(':'))
            return false;
        if (expression.Contains("??"))
            return false;
            
        // Find the ? that's not inside quotes
        var questionIndex = FindOperatorOutsideQuotes(expression, '?');
        if (questionIndex == -1) return false;
        
        // Find the : after the ?
        var colonIndex = FindOperatorOutsideQuotes(expression[(questionIndex + 1)..], ':');
        return colonIndex != -1;
    }

    /// <summary>
    /// Finds an operator character that's not inside quotes.
    /// </summary>
    private static int FindOperatorOutsideQuotes(string expression, char op)
    {
        var inSingleQuote = false;
        var inDoubleQuote = false;
        var parenDepth = 0;
        
        for (int i = 0; i < expression.Length; i++)
        {
            var c = expression[i];
            
            if (c == '\'' && !inDoubleQuote) inSingleQuote = !inSingleQuote;
            else if (c == '"' && !inSingleQuote) inDoubleQuote = !inDoubleQuote;
            else if (c == '(' && !inSingleQuote && !inDoubleQuote) parenDepth++;
            else if (c == ')' && !inSingleQuote && !inDoubleQuote) parenDepth--;
            else if (c == op && !inSingleQuote && !inDoubleQuote && parenDepth == 0)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Evaluates a ternary expression: condition ? trueValue : falseValue
    /// Supports conditions like: var != null, var == null, var == 'value', var (truthy check)
    /// Supports string concatenation in values: 'prefix' + var + 'suffix'
    /// </summary>
    private string EvaluateTernary(string expression, ToolContext context)
    {
        var questionIndex = FindOperatorOutsideQuotes(expression, '?');
        if (questionIndex == -1) return string.Empty;
        
        var condition = expression[..questionIndex].Trim();
        var remainder = expression[(questionIndex + 1)..];
        
        var colonIndex = FindOperatorOutsideQuotes(remainder, ':');
        if (colonIndex == -1) return string.Empty;
        
        var trueExpr = remainder[..colonIndex].Trim();
        var falseExpr = remainder[(colonIndex + 1)..].Trim();
        
        var conditionResult = EvaluateTernaryCondition(condition, context);
        var resultExpr = conditionResult ? trueExpr : falseExpr;

        _logger.LogDebug("Ternary: '{Condition}' → {Result}, using {Branch}",
            condition, conditionResult, conditionResult ? "true-branch" : "false-branch");

        return EvaluateTernaryValue(resultExpr, context);
    }

    /// <summary>
    /// Evaluates a ternary condition.
    /// Supports: var != null, var == null, var != 'value', var == 'value', var (truthy)
    /// </summary>
    private bool EvaluateTernaryCondition(string condition, ToolContext context)
    {
        condition = condition.Trim();
        
        // Check for != null
        if (condition.Contains("!=") && condition.EndsWith("null", StringComparison.OrdinalIgnoreCase))
        {
            var varExpr = condition.Split("!=")[0].Trim();
            var value = ResolveExpression(varExpr, context);
            return value != null;
        }
        
        // Check for == null
        if (condition.Contains("==") && condition.EndsWith("null", StringComparison.OrdinalIgnoreCase))
        {
            var varExpr = condition.Split("==")[0].Trim();
            var value = ResolveExpression(varExpr, context);
            return value == null;
        }
        
        // Check for != 'value' or != "value"
        if (condition.Contains("!="))
        {
            var parts = condition.Split("!=", 2);
            var leftValue = ResolveExpression(parts[0].Trim(), context);
            var rightValue = parts[1].Trim().Trim('\'', '"');
            return leftValue?.ToString() != rightValue;
        }
        
        // Check for == 'value' or == "value"
        if (condition.Contains("=="))
        {
            var parts = condition.Split("==", 2);
            var leftValue = ResolveExpression(parts[0].Trim(), context);
            var rightValue = parts[1].Trim().Trim('\'', '"');
            return leftValue?.ToString() == rightValue;
        }
        
        // Truthy check - variable exists and is not null/empty/false
        var truthyValue = ResolveExpression(condition, context);
        return IsTruthy(truthyValue);
    }

    /// <summary>
    /// Checks if a value is "truthy".
    /// </summary>
    private static bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            string s => !string.IsNullOrEmpty(s),
            int i => i != 0,
            long l => l != 0,
            double d => d != 0,
            _ => true
        };
    }

    /// <summary>
    /// Evaluates a ternary value expression which may contain string concatenation.
    /// Supports: 'literal', variable, 'prefix' + variable + 'suffix'
    /// </summary>
    private string EvaluateTernaryValue(string expr, ToolContext context)
    {
        expr = expr.Trim();
        
        // Check for string concatenation with +
        if (expr.Contains('+'))
        {
            var parts = SplitByPlusOutsideQuotes(expr);
            var result = new System.Text.StringBuilder();
            
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                
                // Check if it's a quoted string
                if ((trimmed.StartsWith('\'') && trimmed.EndsWith('\'')) ||
                    (trimmed.StartsWith('"') && trimmed.EndsWith('"')))
                {
                    result.Append(trimmed[1..^1]);
                }
                else
                {
                    // It's a variable reference
                    var value = ResolveExpression(trimmed, context);
                    result.Append(value?.ToString() ?? string.Empty);
                }
            }
            return result.ToString();
        }
        
        // Single value - either quoted string or variable
        if ((expr.StartsWith('\'') && expr.EndsWith('\'')) ||
            (expr.StartsWith('"') && expr.EndsWith('"')))
        {
            return expr[1..^1];
        }
        
        // Variable reference
        var resolved = ResolveExpression(expr, context);
        return resolved?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Splits an expression by + operator, respecting quotes.
    /// </summary>
    private static List<string> SplitByPlusOutsideQuotes(string expression)
    {
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();
        var inSingleQuote = false;
        var inDoubleQuote = false;
        
        foreach (var c in expression)
        {
            if (c == '\'' && !inDoubleQuote) inSingleQuote = !inSingleQuote;
            else if (c == '"' && !inSingleQuote) inDoubleQuote = !inDoubleQuote;
            
            if (c == '+' && !inSingleQuote && !inDoubleQuote)
            {
                parts.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        
        if (current.Length > 0)
            parts.Add(current.ToString());
            
        return parts;
    }

    /// <summary>
    /// Resolves an expression that may contain pipe helpers.
    /// </summary>
    private object? ResolveExpressionWithPipes(string expression, ToolContext context)
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

        // Apply each helper in sequence. The pipe syntax is parsed by hand here: the
        // regex form allocated a MatchCollection per expression, which dominated the
        // per-render allocation for multi-helper templates.
        var span = helpersExpr.AsSpan();
        var index = 0;
        while (index < span.Length)
        {
            var pipe = span[index..].IndexOf('|');
            if (pipe < 0) break;

            index += pipe + 1;
            var start = index;
            while (index < span.Length && span[index] != '|') index++;

            // Leading whitespace before the helper name is insignificant; the argument
            // keeps its exact text, including a trailing space such as `join:, `.
            var segment = span[start..index].TrimStart();
            if (segment.IsEmpty) continue;

            var colon = segment.IndexOf(':');
            var nameSpan = colon < 0 ? segment : segment[..colon];
            var nameLength = 0;
            while (nameLength < nameSpan.Length
                   && (char.IsLetterOrDigit(nameSpan[nameLength]) || nameSpan[nameLength] == '_'))
            {
                nameLength++;
            }

            var helperName = nameSpan[..nameLength].ToString();
            var helperArg = colon < 0 ? null : segment[(colon + 1)..].ToString();

            // A space before the next pipe is decoration, so it is not part of the
            // argument; a trailing space at the end of the expression is the argument,
            // as in `join:, ` where the separator really is a comma and a space.
            if (helperArg is not null && index < span.Length)
            {
                helperArg = helperArg.TrimEnd();
            }

            if (!TemplateHelpers.Exists(helperName))
            {
                _logger.LogWarning("Unknown pipe helper '{Helper}', value unchanged", helperName);
            }

            value = TemplateHelpers.Apply(value, helperName, helperArg);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Pipe helper '{Helper}' applied", helperName);
            }
        }

        return value;
    }

    /// <summary>
    /// Resolves a variable expression and returns the typed value.
    /// </summary>
    public object? ResolveExpression(string expression, ToolContext context)
    {
        // An expression may chain pipe helpers, exactly as a ${...} token does.
        if (expression.Contains('|'))
        {
            return ResolveExpressionWithPipes(expression, context);
        }

        // Handle Context.Get("key") syntax
        if (expression.StartsWith("Context.Get(", StringComparison.OrdinalIgnoreCase))
        {
            var keyMatch = _contextGetPattern.Match(expression);
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
    private object? ResolvePath(string path, ToolContext context)
    {
        // Check if path contains method calls
        if (path.Contains('('))
        {
            return ResolvePathWithMethodCalls(path, context);
        }

        // A single key needs no path split at all, which is the common case.
        if (path.IndexOf('.') < 0)
        {
            return context.Get(path);
        }

        var parts = path.Split('.');
        if (parts.Length == 0) return null;

        // First part is the context key
        var currentValue = context.Get(parts[0]);
        if (currentValue == null)
        {
            _logger.LogWarning("Context key '{Key}' not found, resolved to \"\"", parts[0]);
            return null;
        }

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
    private object? ResolvePathWithMethodCalls(string path, ToolContext context)
    {
        // Start with context as current value
        object? currentValue = context;
        var index = 0;

        // Pattern to match method call: methodName('arg') or methodName("arg")
        var methodCallPattern = MethodCallPattern();
        // Pattern to match direct invocation with string literal: ('arg') or ("arg")
        var directCallLiteralPattern = DirectCallLiteralPattern();
        // Pattern to match direct invocation with variable: (varName) or (Context.Path.To.Value)
        var directCallVariablePattern = DirectCallVariablePattern();

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
    private object? InvokeMethodOrProperty(object? currentValue, string methodName, string argument, ToolContext context)
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
    private object? InvokeDelegate(object? currentValue, object? argument, ToolContext context)
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
        if (obj is ToolContext ctx)
        {
            return ctx.Get(propertyName);
        }

        var type = obj.GetType();
        var member = Members.GetOrAdd((type, propertyName), static key => new MemberCacheEntry(
            key.Type.GetProperty(key.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase),
            key.Type.GetField(key.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)));

        if (member.Property != null)
        {
            return member.Property.GetValue(obj);
        }

        if (member.Field != null)
        {
            return member.Field.GetValue(obj);
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
    /// Caches the reflected member per type and name. Reflection lookups dominated
    /// variable resolution, and the same few paths repeat for every render.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(Type Type, string Name), MemberCacheEntry> Members = new();

    private readonly record struct MemberCacheEntry(PropertyInfo? Property, FieldInfo? Field);

    private static readonly Regex _methodCallPattern = new(@"^(\w+)\('([^']*)'\)|^(\w+)\(""([^""]*)""\)", RegexOptions.Compiled);
    private static Regex MethodCallPattern() => _methodCallPattern;

    private static readonly Regex _directCallLiteralPattern = new(@"^\('([^']*)'\)|^\(""([^""]*)""\)", RegexOptions.Compiled);
    private static Regex DirectCallLiteralPattern() => _directCallLiteralPattern;

    private static readonly Regex _directCallVariablePattern = new(@"^\(([^)]+)\)", RegexOptions.Compiled);
    private static Regex DirectCallVariablePattern() => _directCallVariablePattern;

    private static readonly Regex _contextGetPattern = new(@"Context\.Get\([""']([^""']+)[""']\)", RegexOptions.Compiled);

    /// <summary>
    /// Resolves inputs dictionary, replacing any ${Context.xxx} expressions.
    /// </summary>
    public Dictionary<string, object?> ResolveInputs(Dictionary<string, object?> inputs, ToolContext context)
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
    public object? ResolveValue(object? value, ToolContext context)
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
