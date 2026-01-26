using System.Text;
using System.Text.RegularExpressions;

namespace LowlandTech.TinyTools;

/// <summary>
/// Simple template engine that handles ${Context.xxx} interpolation and @if/@foreach control flow.
/// </summary>
public partial class TinyTemplateEngine : ITemplateEngine
{
    private readonly VariableResolver _resolver = new();

    public string Render(string template, ExecutionContext context)
    {
        if (string.IsNullOrEmpty(template)) return template;

        // First, process control flow (@if, @foreach)
        var processed = ProcessControlFlow(template, context);

        // Then, resolve variables
        return ResolveVariables(processed, context);
    }

    public string ResolveVariables(string input, ExecutionContext context)
    {
        return _resolver.ResolveString(input, context);
    }

    private string ProcessControlFlow(string template, ExecutionContext context)
    {
        // First, remove comments (@* ... *@)
        template = RemoveComments(template);
        
        var result = new StringBuilder();
        var lines = template.Split('\n');
        var lineIndex = 0;

        while (lineIndex < lines.Length)
        {
            var line = lines[lineIndex];
            var trimmed = line.TrimStart();

            if (trimmed.StartsWith("@if"))
            {
                var (processed, newIndex) = ProcessIfBlock(lines, lineIndex, context);
                result.Append(processed);
                lineIndex = newIndex;
            }
            else if (trimmed.StartsWith("@foreach"))
            {
                var (processed, newIndex) = ProcessForeachBlock(lines, lineIndex, context);
                result.Append(processed);
                lineIndex = newIndex;
            }
            else
            {
                result.AppendLine(line);
                lineIndex++;
            }
        }

        // Remove trailing newline if original didn't have one
        var resultStr = result.ToString();
        if (!template.EndsWith('\n') && resultStr.EndsWith(Environment.NewLine))
        {
            resultStr = resultStr.TrimEnd('\r', '\n');
        }

        return resultStr;
    }

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex CommentPattern();

    /// <summary>
    /// Removes template comments (@* ... *@) from the template.
    /// </summary>
    private static string RemoveComments(string template)
    {
        return CommentPattern().Replace(template, string.Empty);
    }

    [GeneratedRegex(@"@if\s*\((.+?)\)\s*\{", RegexOptions.Compiled)]
    private static partial Regex IfPattern();

    [GeneratedRegex(@"@foreach\s*\(\s*var\s+(\w+)\s+in\s+([^)]+)\)\s*\{", RegexOptions.Compiled)]
    private static partial Regex ForeachPattern();

    private (string content, int endIndex) ProcessIfBlock(string[] lines, int startIndex, ExecutionContext context)
    {
        var line = lines[startIndex];
        var match = IfPattern().Match(line);
        if (!match.Success)
        {
            return (line + "\n", startIndex + 1);
        }

        var condition = match.Groups[1].Value.Trim();
        var conditionResult = EvaluateCondition(condition, context);

        // Find matching closing brace and potential else/else-if
        var (blocks, endIndex) = ExtractIfElseIfBlocks(lines, startIndex);

        if (conditionResult)
        {
            return (ProcessControlFlow(blocks[0].Content, context), endIndex);
        }

        // Check else-if and else blocks
        for (int i = 1; i < blocks.Count; i++)
        {
            var block = blocks[i];
            if (block.Condition == null)
            {
                // This is an else block
                return (ProcessControlFlow(block.Content, context), endIndex);
            }
            else if (EvaluateCondition(block.Condition, context))
            {
                // This is an else-if block that matches
                return (ProcessControlFlow(block.Content, context), endIndex);
            }
        }

        return (string.Empty, endIndex);
    }

    [GeneratedRegex(@"}\s*else\s+if\s*\(([^)]+)\)\s*\{", RegexOptions.Compiled)]
    private static partial Regex ElseIfPattern();

    private record ConditionalBlock(string? Condition, string Content);

    private (List<ConditionalBlock> blocks, int endIndex) ExtractIfElseIfBlocks(string[] lines, int startIndex)
    {
        var blocks = new List<ConditionalBlock>();
        var currentLines = new List<string>();
        string? currentCondition = null;
        var braceCount = 0;
        var inBlock = false;
        var i = startIndex;

        for (; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.TrimStart();

            // Check for "} else if (...) {" pattern
            var elseIfMatch = ElseIfPattern().Match(trimmed);
            if (inBlock && braceCount == 1 && elseIfMatch.Success)
            {
                // Save current block and start new else-if block
                blocks.Add(new ConditionalBlock(currentCondition, string.Join("\n", currentLines)));
                currentLines.Clear();
                currentCondition = elseIfMatch.Groups[1].Value.Trim();
                continue;
            }

            // Check for "} else {" pattern
            if (inBlock && braceCount == 1 && (trimmed.StartsWith("} else {") || trimmed.StartsWith("} else{")))
            {
                // Save current block and start else block
                blocks.Add(new ConditionalBlock(currentCondition, string.Join("\n", currentLines)));
                currentLines.Clear();
                currentCondition = null; // null indicates else block
                continue;
            }

            // Count braces
            foreach (var c in line)
            {
                if (c == '{') { braceCount++; inBlock = true; }
                else if (c == '}') braceCount--;
            }

            // Check for block end
            if (braceCount == 0 && inBlock)
            {
                blocks.Add(new ConditionalBlock(currentCondition, string.Join("\n", currentLines)));
                return (blocks, i + 1);
            }

            // Skip the @if line itself
            if (i == startIndex) continue;

            // Don't add closing brace line
            if (!(braceCount == 0 && line.Trim() == "}"))
                currentLines.Add(line);
        }

        blocks.Add(new ConditionalBlock(currentCondition, string.Join("\n", currentLines)));
        return (blocks, i);
    }

    private (string content, int endIndex) ProcessForeachBlock(string[] lines, int startIndex, ExecutionContext context)
    {
        var line = lines[startIndex];
        var match = ForeachPattern().Match(line);
        if (!match.Success)
        {
            return (line + "\n", startIndex + 1);
        }

        var itemVar = match.Groups[1].Value;
        var collectionExpr = match.Groups[2].Value.Trim();

        // Get the collection
        var collection = _resolver.ResolveExpression(collectionExpr, context);
        if (collection == null)
        {
            return (string.Empty, FindClosingBrace(lines, startIndex));
        }

        // Extract block content
        var (blockContent, endIndex) = ExtractBlock(lines, startIndex);

        // Iterate and render
        var result = new StringBuilder();
        
        // Don't iterate over strings as IEnumerable (would iterate chars)
        if (collection is string)
        {
            throw new InvalidOperationException($"Cannot iterate over string value in @foreach. Collection expression: {collectionExpr}");
        }
        
        var items = collection as System.Collections.IEnumerable;
        if (items != null)
        {
            foreach (var item in items)
            {
                // Create child context with item variable
                var childContext = context.CreateChild();
                childContext.Set(itemVar, item);

                var rendered = ProcessControlFlow(blockContent, childContext);
                rendered = _resolver.ResolveString(rendered, childContext);
                result.Append(rendered);
            }
        }

        return (result.ToString(), endIndex);
    }

    private (string content, int endIndex) ExtractBlock(string[] lines, int startIndex)
    {
        var braceCount = 0;
        var blockLines = new List<string>();
        var inBlock = false;

        for (var i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i];

            foreach (var c in line)
            {
                if (c == '{') { braceCount++; inBlock = true; }
                else if (c == '}') braceCount--;
            }

            // Skip the @foreach/@if line itself
            if (i == startIndex) continue;

            // Check if we've closed the block
            if (braceCount == 0 && inBlock)
            {
                return (string.Join("\n", blockLines), i + 1);
            }

            blockLines.Add(line);
        }

        return (string.Join("\n", blockLines), lines.Length);
    }

    private int FindClosingBrace(string[] lines, int startIndex)
    {
        var braceCount = 0;
        for (var i = startIndex; i < lines.Length; i++)
        {
            foreach (var c in lines[i])
            {
                if (c == '{') braceCount++;
                else if (c == '}') braceCount--;
            }
            if (braceCount == 0 && i > startIndex) return i + 1;
        }
        return lines.Length;
    }

    private bool EvaluateCondition(string condition, ExecutionContext context)
    {
        // Simple condition evaluation
        // Supports: !expr (negation), expr > 0, expr == value, expr != value, expr (truthy check)

        condition = condition.Trim();

        // Handle negation (! operator)
        if (condition.StartsWith('!'))
        {
            var innerCondition = condition[1..].Trim();
            // Remove parentheses if present: !(expr) -> expr
            if (innerCondition.StartsWith('(') && innerCondition.EndsWith(')'))
            {
                innerCondition = innerCondition[1..^1];
            }
            return !EvaluateCondition(innerCondition, context);
        }

        // Check for comparison operators (longer operators first to avoid partial matches)
        if (condition.Contains(" >= "))
        {
            var parts = condition.Split(" >= ");
            var left = _resolver.ResolveExpression(parts[0].Trim(), context);
            var right = ResolveValueOrExpression(parts[1].Trim(), context);
            return Compare(left, right) >= 0;
        }

        if (condition.Contains(" <= "))
        {
            var parts = condition.Split(" <= ");
            var left = _resolver.ResolveExpression(parts[0].Trim(), context);
            var right = ResolveValueOrExpression(parts[1].Trim(), context);
            return Compare(left, right) <= 0;
        }

        if (condition.Contains(" > "))
        {
            var parts = condition.Split(" > ");
            var left = _resolver.ResolveExpression(parts[0].Trim(), context);
            var right = ResolveValueOrExpression(parts[1].Trim(), context);
            return Compare(left, right) > 0;
        }

        if (condition.Contains(" < "))
        {
            var parts = condition.Split(" < ");
            var left = _resolver.ResolveExpression(parts[0].Trim(), context);
            var right = ResolveValueOrExpression(parts[1].Trim(), context);
            return Compare(left, right) < 0;
        }

        if (condition.Contains(" == "))
        {
            var parts = condition.Split(" == ");
            var left = _resolver.ResolveExpression(parts[0].Trim(), context);
            var right = ResolveValueOrExpression(parts[1].Trim(), context);
            return AreEqual(left, right);
        }

        if (condition.Contains(" != "))
        {
            var parts = condition.Split(" != ");
            var left = _resolver.ResolveExpression(parts[0].Trim(), context);
            var right = ResolveValueOrExpression(parts[1].Trim(), context);
            return !AreEqual(left, right);
        }

        // Truthy check
        var value = _resolver.ResolveExpression(condition, context);
        return IsTruthy(value);
    }

    private object? ResolveValueOrExpression(string value, ExecutionContext context)
    {
        // If it looks like a context expression (contains 'Context.'), resolve it
        if (value.Contains("Context."))
        {
            return _resolver.ResolveExpression(value, context);
        }
        // Otherwise, treat it as a literal value
        return ParseValue(value);
    }

    private static object? ParseValue(string value)
    {
        value = value.Trim().Trim('"', '\'');
        if (int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var intVal)) return intVal;
        if (double.TryParse(value, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out var doubleVal)) return doubleVal;
        if (bool.TryParse(value, out var boolVal)) return boolVal;
        if (value == "null") return null;
        return value;
    }

    private static int Compare(object? left, object? right)
    {
        if (left == null && right == null) return 0;
        if (left == null) return -1;
        if (right == null) return 1;

        if (IsNumeric(left) && IsNumeric(right))
        {
            return Convert.ToDouble(left).CompareTo(Convert.ToDouble(right));
        }

        return string.Compare(left.ToString(), right?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        if (IsNumeric(left) && IsNumeric(right))
        {
            // Use tolerance-based comparison for floating-point values
            var leftValue = Convert.ToDouble(left);
            var rightValue = Convert.ToDouble(right);
            return Math.Abs(leftValue - rightValue) < 1e-10;
        }
        return string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTruthy(object? value)
    {
        if (value == null) return false;
        if (value is bool b) return b;
        if (value is int i) return i != 0;
        if (value is string s) return !string.IsNullOrEmpty(s);
        if (value is System.Collections.ICollection c) return c.Count > 0;
        return true;
    }

    private static bool IsNumeric(object? value)
    {
        return value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }
}
