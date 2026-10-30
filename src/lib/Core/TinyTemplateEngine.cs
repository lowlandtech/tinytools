using System.Text;

namespace LowlandTech.TinyTools;

/// <summary>
/// Simple template engine that handles ${Context.xxx} interpolation and @if/@foreach control flow.
/// </summary>
public partial class TinyTemplateEngine : ITemplateEngine, ITemplateCompiler
{
    private const int CompiledPlanCapacity = 128;

    private readonly VariableResolver _resolver;
    private readonly ILogger _logger;
    private readonly TemplatePlanCache _plans = new(CompiledPlanCapacity);

    public TinyTemplateEngine(ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.CreateLogger<TinyTemplateEngine>()
                  ?? NullLogger<TinyTemplateEngine>.Instance;
        _resolver = new VariableResolver(loggerFactory);
    }

    public string Render(string template, ToolContext context)
    {
        if (string.IsNullOrEmpty(template)) return template;

        _logger.LogTrace("Render called, template length: {Length}", template.Length);

        // First, process control flow (@if, @foreach)
        var processed = ProcessControlFlow(template, context);

        // Then, resolve variables
        var result = ResolveVariables(processed, context);

        _logger.LogTrace("Render complete, output length: {Length}", result.Length);
        return result;
    }

    public string ResolveVariables(string input, ToolContext context)
    {
        return _resolver.ResolveString(input, context);
    }

    /// <summary>Resolves one typed expression for workflow orchestration.</summary>
    public object? ResolveExpression(string expression, ToolContext context)
    {
        return _resolver.ResolveExpression(expression, context);
    }

    /// <summary>
    /// Compiles the source once and reuses the plan for that exact source until it is
    /// evicted. Compilation normalizes line endings and nothing else - it never
    /// resolves variables, calls a helper or captures a context, and rendered output
    /// is never cached.
    /// </summary>
    public CompiledTemplate Compile(string source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        if (_plans.TryGet(source, out var cached) && cached is not null) return cached;

        return _plans.Add(source, () =>
        {
            var normalized = source.Replace("\r\n", "\n").Replace("\r", "\n");
            return new CompiledTemplate(source, normalized, context => Render(normalized, context), this);
        });
    }

    /// <summary>
    /// Validates the supported syntax without rendering. Diagnostics are stable codes
    /// carrying a one-based line and column and the offending construct:
    /// TS001 unterminated <c>${</c>, TS002 unterminated comment, TS003 control-flow
    /// statement without a condition, TS004 control-flow statement without an opening
    /// brace, TS005 control-flow block that is never closed, TS007 <c>@else</c> with no
    /// open <c>@if</c>.
    /// </summary>
    public TemplateSyntaxValidationResult ValidateSyntax(string template)
    {
        if (string.IsNullOrEmpty(template)) return TemplateSyntaxValidationResult.Valid;

        var diagnostics = new List<TemplateSyntaxDiagnostic>();
        var text = template.Replace("\r\n", "\n").Replace("\r", "\n");
        text = StripComments(text, diagnostics);

        for (var index = text.IndexOf("${", StringComparison.Ordinal); index >= 0;
             index = text.IndexOf("${", index + 2, StringComparison.Ordinal))
        {
            if (text.IndexOf('}', index) < 0)
            {
                var (line, column) = Locate(text, index);
                diagnostics.Add(new("TS001", "An interpolation opened with '${' is never closed.", line, column, "${"));
                break;
            }
        }

        var lines = text.Split('\n');
        var covered = new bool[lines.Length];
        for (var i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            var line = raw.TrimStart();
            var isIf = line.StartsWith("@if", StringComparison.Ordinal);
            var isForeach = line.StartsWith("@foreach", StringComparison.Ordinal);
            if (!isIf && !isForeach) continue;

            var keyword = isIf ? "@if" : "@foreach";
            var column = raw.Length - line.Length + 1;

            if (!line.Contains('(') || !line.Contains(')'))
            {
                diagnostics.Add(new("TS003", $"{keyword} on line {i + 1} has no condition.", i + 1, column, keyword));
                continue;
            }

            if (!line.Contains('{'))
            {
                diagnostics.Add(new("TS004", $"{keyword} on line {i + 1} has no opening brace.", i + 1, column, keyword));
                continue;
            }

            var end = FindBlockEnd(lines, i);
            if (end < 0)
            {
                diagnostics.Add(new("TS005", $"{keyword} opened on line {i + 1} is never closed.", i + 1, column, keyword));
                continue;
            }

            for (var coveredLine = i; coveredLine <= end && coveredLine < covered.Length; coveredLine++)
            {
                covered[coveredLine] = true;
            }
        }

        for (var i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            var line = raw.TrimStart();
            if (!line.StartsWith("@else", StringComparison.Ordinal) || covered[i]) continue;
            if (i > 0 && covered[i - 1]) continue;

            diagnostics.Add(new("TS007", $"@else on line {i + 1} has no open @if block.", i + 1, raw.Length - line.Length + 1, "@else"));
        }

        return diagnostics.Count == 0
            ? TemplateSyntaxValidationResult.Valid
            : new TemplateSyntaxValidationResult(false, diagnostics);
    }

    /// <summary>Maps an absolute index in the normalized text to a one-based line and column.</summary>
    private static (int Line, int Column) Locate(string text, int index)
    {
        var line = 1;
        var column = 1;
        for (var i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        return (line, column);
    }

    private static string StripComments(string text, List<TemplateSyntaxDiagnostic> diagnostics)
    {
        var open = text.IndexOf("@*", StringComparison.Ordinal);
        while (open >= 0)
        {
            var close = text.IndexOf("*@", open + 2, StringComparison.Ordinal);
            if (close < 0)
            {
                diagnostics.Add(new("TS002", "A comment opened with '@*' is never closed."));
                return text;
            }

            text = text.Remove(open, close + 2 - open);
            open = text.IndexOf("@*", StringComparison.Ordinal);
        }

        return text;
    }

    /// <summary>
    /// Finds the line that closes the block opened at <paramref name="startIndex"/>,
    /// using the same brace counting the renderer uses, or -1 when it never closes.
    /// </summary>
    private static int FindBlockEnd(string[] lines, int startIndex)
    {
        var depth = 0;
        var opened = false;

        for (var i = startIndex; i < lines.Length; i++)
        {
            foreach (var character in lines[i])
            {
                if (character == '{') { depth++; opened = true; }
                else if (character == '}') depth--;
            }

            if (i == startIndex) continue;
            if (opened && depth == 0) return i;
            if (depth < 0) return -1;
        }

        return -1;
    }

    private string ProcessControlFlow(string template, ToolContext context)
    {
        // First, remove comments (@* ... *@)
        template = RemoveComments(template);
        
        // Normalize line endings to \n for consistent processing
        template = template.Replace("\r\n", "\n").Replace("\r", "\n");
        
        // Every plain line is emitted with the canonical LF separator, so output does
        // not vary by host platform. A block replaces whole source lines and carries
        // its own terminating line break, so one is only appended when the block
        // rendered text without one; an empty block emitted no line and must not add
        // a boundary either.
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
                AppendBlock(result, processed, newIndex < lines.Length);
                lineIndex = newIndex;
            }
            else if (trimmed.StartsWith("@foreach"))
            {
                var (processed, newIndex) = ProcessForeachBlock(lines, lineIndex, context);
                AppendBlock(result, processed, newIndex < lines.Length);
                lineIndex = newIndex;
            }
            else
            {
                result.Append(line);
                if (lineIndex < lines.Length - 1)
                {
                    result.Append('\n');
                }
                lineIndex++;
            }
        }

        // Split leaves a final empty line when the template ends with a line break, so
        // a template without one has to lose the separator this loop added.
        var resultStr = result.ToString();
        return template.EndsWith('\n') ? resultStr : resultStr.TrimEnd('\n');
    }

    private static void AppendBlock(StringBuilder result, string processed, bool hasFollowingLine)
    {
        result.Append(processed);
        if (hasFollowingLine && processed.Length > 0 && !processed.EndsWith('\n'))
        {
            result.Append('\n');
        }
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex CommentPattern();
#else
    private static readonly Regex _commentPattern = new Regex(@"@\*.*?\*@", RegexOptions.Singleline | RegexOptions.Compiled);
    private static Regex CommentPattern() => _commentPattern;
#endif

    /// <summary>
    /// Removes template comments (@* ... *@) from the template.
    /// </summary>
    private static string RemoveComments(string template)
    {
        return CommentPattern().Replace(template, string.Empty);
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"@if\s*\((.+?)\)\s*\{", RegexOptions.Compiled)]
    private static partial Regex IfPattern();
#else
    private static readonly Regex _ifPattern = new Regex(@"@if\s*\((.+?)\)\s*\{", RegexOptions.Compiled);
    private static Regex IfPattern() => _ifPattern;
#endif

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"@if\s*\((.+?)\)\s*$", RegexOptions.Compiled)]
    private static partial Regex IfEndPattern();
#else
    private static readonly Regex _ifEndPattern = new Regex(@"@if\s*\((.+?)\)\s*$", RegexOptions.Compiled);
    private static Regex IfEndPattern() => _ifEndPattern;
#endif

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"@foreach\s*\(\s*var\s+(\w+)\s+in\s+([^)]+)\)\s*\{", RegexOptions.Compiled)]
    private static partial Regex ForeachPattern();
#else
    private static readonly Regex _foreachPattern = new Regex(@"@foreach\s*\(\s*var\s+(\w+)\s+in\s+([^)]+)\)\s*\{", RegexOptions.Compiled);
    private static Regex ForeachPattern() => _foreachPattern;
#endif

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"@foreach\s*\(\s*var\s+(\w+)\s+in\s+([^)]+?)\)\s*$", RegexOptions.Compiled)]
    private static partial Regex ForeachEndPattern();
#else
    private static readonly Regex _foreachEndPattern = new Regex(@"@foreach\s*\(\s*var\s+(\w+)\s+in\s+([^)]+?)\)\s*$", RegexOptions.Compiled);
    private static Regex ForeachEndPattern() => _foreachEndPattern;
#endif

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^@else\s+if\s*\((.+?)\)\s*$", RegexOptions.Compiled)]
    private static partial Regex ElseIfEndPattern();
#else
    private static readonly Regex _elseIfEndPattern = new Regex(@"^@else\s+if\s*\((.+?)\)\s*$", RegexOptions.Compiled);
    private static Regex ElseIfEndPattern() => _elseIfEndPattern;
#endif

    private static bool UsesEndSyntax(string line) => !line.TrimEnd().EndsWith('{');

    private (string content, int endIndex) ProcessIfBlock(string[] lines, int startIndex, ToolContext context)
    {
        var line = lines[startIndex];

        // Try @end style first (no opening brace), then brace style
        var endMatch = IfEndPattern().Match(line.TrimStart());
        var braceMatch = IfPattern().Match(line);

        var match = endMatch.Success && UsesEndSyntax(line) ? endMatch : braceMatch;
        if (!match.Success)
        {
            return (line + "\n", startIndex + 1);
        }

        var useEndSyntax = endMatch.Success && UsesEndSyntax(line);
        var condition = match.Groups[1].Value.Trim();
        var conditionResult = EvaluateCondition(condition, context);

        _logger.LogDebug("@if ({Condition}) → {Result}", condition, conditionResult);

        var (blocks, endIndex) = useEndSyntax
            ? ExtractIfElseIfBlocksEnd(lines, startIndex)
            : ExtractIfElseIfBlocks(lines, startIndex);

        if (conditionResult)
        {
            _logger.LogDebug("@if: taking primary branch");
            return (ProcessControlFlow(blocks[0].Content, context), endIndex);
        }

        // Check else-if and else blocks
        for (int i = 1; i < blocks.Count; i++)
        {
            var block = blocks[i];
            if (block.Condition == null)
            {
                _logger.LogDebug("@if: taking @else branch (block {Index})", i);
                return (ProcessControlFlow(block.Content, context), endIndex);
            }
            else if (EvaluateCondition(block.Condition, context))
            {
                _logger.LogDebug("@if: taking @else if ({Condition}) branch (block {Index})", block.Condition, i);
                return (ProcessControlFlow(block.Content, context), endIndex);
            }
        }

        _logger.LogDebug("@if: no branch taken, emitting empty");
        return (string.Empty, endIndex);
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"}\s*else\s+if\s*\(([^)]+)\)\s*\{", RegexOptions.Compiled)]
    private static partial Regex ElseIfPattern();
#else
    private static readonly Regex _elseIfPattern = new Regex(@"}\s*else\s+if\s*\(([^)]+)\)\s*\{", RegexOptions.Compiled);
    private static Regex ElseIfPattern() => _elseIfPattern;
#endif


    /// <summary>
    /// Joins a block's body lines back into template text. The closing brace always
    /// occupies its own source line, so a non-empty body is terminated by the line
    /// break that preceded that brace - without it, a blank line at the end of a loop
    /// body would collapse and an @if body would lose the boundary that follows it.
    /// </summary>
    private static string BlockText(List<string> lines) =>
        lines.Count == 0 ? string.Empty : string.Join("\n", lines) + "\n";
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
                blocks.Add(new ConditionalBlock(currentCondition, BlockText(currentLines)));
                currentLines.Clear();
                currentCondition = elseIfMatch.Groups[1].Value.Trim();
                continue;
            }

            // Check for "} else {" pattern
            if (inBlock && braceCount == 1 && (trimmed.StartsWith("} else {") || trimmed.StartsWith("} else{")))
            {
                // Save current block and start else block
                blocks.Add(new ConditionalBlock(currentCondition, BlockText(currentLines)));
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
                blocks.Add(new ConditionalBlock(currentCondition, BlockText(currentLines)));
                return (blocks, i + 1);
            }

            // Skip the @if line itself
            if (i == startIndex) continue;

            // Don't add closing brace line
            if (!(braceCount == 0 && line.Trim() == "}"))
                currentLines.Add(line);
        }

        blocks.Add(new ConditionalBlock(currentCondition, BlockText(currentLines)));
        return (blocks, i);
    }

    private (string content, int endIndex) ProcessForeachBlock(string[] lines, int startIndex, ToolContext context)
    {
        var line = lines[startIndex];

        // Try @end style first (no opening brace), then brace style
        var endMatch = ForeachEndPattern().Match(line.TrimStart());
        var braceMatch = ForeachPattern().Match(line);

        var match = endMatch.Success && UsesEndSyntax(line) ? endMatch : braceMatch;
        if (!match.Success)
        {
            return (line + "\n", startIndex + 1);
        }

        var useEndSyntax = endMatch.Success && UsesEndSyntax(line);
        var itemVar = match.Groups[1].Value;
        var collectionExpr = match.Groups[2].Value.Trim();

        // Get the collection
        var collection = _resolver.ResolveExpression(collectionExpr, context);
        if (collection == null)
        {
            _logger.LogWarning("@foreach var {Var} in {Collection} → null, skipping block", itemVar, collectionExpr);
            var skipEnd = useEndSyntax ? FindEndMarker(lines, startIndex) : FindClosingBrace(lines, startIndex);
            return (string.Empty, skipEnd);
        }

        // Extract block content
        var (blockContent, endIndex) = useEndSyntax
            ? ExtractBlockEnd(lines, startIndex)
            : ExtractBlock(lines, startIndex);

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
            // Materialize to a list so we can provide _index, _first, _last metadata
            var itemList = new List<object?>();
            foreach (var item in items)
                itemList.Add(item);

            _logger.LogDebug("@foreach var {Var} in {Collection} → {Count} items", itemVar, collectionExpr, itemList.Count);

            for (var i = 0; i < itemList.Count; i++)
            {
                // Create child context with item variable and loop metadata
                var childContext = context.CreateChild();
                childContext.Set(itemVar, itemList[i]);
                childContext.Set("_index", i);
                childContext.Set("_first", i == 0);
                childContext.Set("_last", i == itemList.Count - 1);
                childContext.Set("_count", itemList.Count);

                var rendered = ProcessControlFlow(blockContent, childContext);
                rendered = _resolver.ResolveString(rendered, childContext);
                result.Append(rendered);

                // Each iteration's content should end with a newline to separate from next iteration
                if (rendered.Length > 0 && !rendered.EndsWith('\n'))
                {
                    result.Append('\n');
                }
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
                return (BlockText(blockLines), i + 1);
            }

            blockLines.Add(line);
        }

        return (BlockText(blockLines), lines.Length);
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

    /// <summary>
    /// Extracts @if/@else if/@else blocks terminated by @end.
    /// Handles nesting by tracking @if/@foreach depth.
    /// </summary>
    private (List<ConditionalBlock> blocks, int endIndex) ExtractIfElseIfBlocksEnd(string[] lines, int startIndex)
    {
        var blocks = new List<ConditionalBlock>();
        var currentLines = new List<string>();
        string? currentCondition = null;
        var depth = 0;

        for (var i = startIndex; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();

            // Skip the opening @if line
            if (i == startIndex)
            {
                depth = 1;
                continue;
            }

            // Track nesting depth
            if (trimmed.StartsWith("@if") || trimmed.StartsWith("@foreach"))
                depth++;

            if (trimmed == "@end")
            {
                depth--;
                if (depth == 0)
                {
                    blocks.Add(new ConditionalBlock(currentCondition, BlockText(currentLines)));
                    return (blocks, i + 1);
                }
            }

            // At top level, check for @else if / @else
            if (depth == 1)
            {
                var elseIfMatch = ElseIfEndPattern().Match(trimmed);
                if (elseIfMatch.Success)
                {
                    blocks.Add(new ConditionalBlock(currentCondition, BlockText(currentLines)));
                    currentLines.Clear();
                    currentCondition = elseIfMatch.Groups[1].Value.Trim();
                    continue;
                }

                if (trimmed == "@else")
                {
                    blocks.Add(new ConditionalBlock(currentCondition, BlockText(currentLines)));
                    currentLines.Clear();
                    currentCondition = null; // null indicates else block
                    continue;
                }
            }

            currentLines.Add(lines[i]);
        }

        blocks.Add(new ConditionalBlock(currentCondition, BlockText(currentLines)));
        return (blocks, lines.Length);
    }

    /// <summary>
    /// Extracts block content for @foreach terminated by @end.
    /// Handles nesting by tracking depth.
    /// </summary>
    private (string content, int endIndex) ExtractBlockEnd(string[] lines, int startIndex)
    {
        var blockLines = new List<string>();
        var depth = 0;

        for (var i = startIndex; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();

            if (i == startIndex)
            {
                depth = 1;
                continue;
            }

            if (trimmed.StartsWith("@if") || trimmed.StartsWith("@foreach"))
                depth++;

            if (trimmed == "@end")
            {
                depth--;
                if (depth == 0)
                    return (BlockText(blockLines), i + 1);
            }

            blockLines.Add(lines[i]);
        }

        return (BlockText(blockLines), lines.Length);
    }

    private int FindEndMarker(string[] lines, int startIndex)
    {
        var depth = 0;
        for (var i = startIndex; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("@if") || trimmed.StartsWith("@foreach"))
                depth++;
            if (trimmed == "@end")
            {
                depth--;
                if (depth == 0) return i + 1;
            }
        }
        return lines.Length;
    }

    private bool EvaluateCondition(string condition, ToolContext context)
    {
        // Simple condition evaluation
        // Supports: && (and), || (or), !expr (negation), expr > 0, expr == value, expr != value, expr (truthy check)

        condition = condition.Trim();

        // Handle logical OR (||) - lowest precedence, evaluate left to right
        var orIndex = FindOperatorOutsideParentheses(condition, "||");
        if (orIndex >= 0)
        {
            var left = condition[..orIndex].Trim();
            var right = condition[(orIndex + 2)..].Trim();
            return EvaluateCondition(left, context) || EvaluateCondition(right, context);
        }

        // Handle logical AND (&&) - higher precedence than OR
        var andIndex = FindOperatorOutsideParentheses(condition, "&&");
        if (andIndex >= 0)
        {
            var left = condition[..andIndex].Trim();
            var right = condition[(andIndex + 2)..].Trim();
            return EvaluateCondition(left, context) && EvaluateCondition(right, context);
        }

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

        // Handle parenthesized expressions
        if (condition.StartsWith('(') && condition.EndsWith(')'))
        {
            return EvaluateCondition(condition[1..^1], context);
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

    /// <summary>
    /// Finds a logical operator (&amp;&amp;, ||) that's not inside parentheses.
    /// </summary>
    private static int FindOperatorOutsideParentheses(string condition, string op)
    {
        var parenDepth = 0;
        for (int i = 0; i <= condition.Length - op.Length; i++)
        {
            var c = condition[i];
            if (c == '(') parenDepth++;
            else if (c == ')') parenDepth--;
            else if (parenDepth == 0 && condition.Substring(i, op.Length) == op)
            {
                return i;
            }
        }
        return -1;
    }

    private object? ResolveValueOrExpression(string value, ToolContext context)
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
