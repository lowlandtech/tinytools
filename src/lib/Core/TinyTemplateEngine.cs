using System.Text;

namespace LowlandTech.TinyTools;

/// <summary>
/// Simple template engine that handles ${Context.xxx} interpolation and @if/@foreach control flow.
/// </summary>
public partial class TinyTemplateEngine : ITemplateEngine, ITemplateCompiler
{
    private const int CompiledPlanCapacity = 128;

    private static readonly System.Text.RegularExpressions.Regex IncludePattern = new(
        """@include\(\s*['"](?<name>[^'"]+)['"]\s*\)""",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    private readonly VariableResolver _resolver;
    private readonly ILogger _logger;
    private readonly TemplatePlanCache _plans = new(CompiledPlanCapacity);
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _fragments = new(StringComparer.Ordinal);

    public TinyTemplateEngine(ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.CreateLogger<TinyTemplateEngine>()
                  ?? NullLogger<TinyTemplateEngine>.Instance;
        _resolver = new VariableResolver(loggerFactory);
    }

    public string Render(string template, ToolContext context) => RenderInternal(template, context, 0, null, Array.Empty<string>());

    private string RenderInternal(string template, ToolContext context, int depth, IReadOnlyDictionary<string, MacroDefinition>? inherited, IReadOnlyList<string> chain)
    {
        if (string.IsNullOrEmpty(template)) return template;

        if (template.Contains("@service(", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Asynchronous service calls require RenderAsync; the synchronous Render path does not block on them.");
        }

        if (depth > MacroRecursionLimit)
        {
            throw new InvalidOperationException(
                $"Macro recursion limit exceeded ({MacroRecursionLimit} nested expansions).");
        }

        _logger.LogTrace("Render called, template length: {Length}", template.Length);

        // Includes expand first: a fragment is plain template source, so the control-flow
        // and variable passes then run over the composed text in one go.
        var composed = ExpandIncludes(template, Array.Empty<string>());

        // Macros then expand their bodies eagerly, binding arguments as context values, so a
        // macro body keeps the full renderer (interpolation and control flow) inside it.
        composed = ExpandMacros(composed, context, depth, inherited, chain);

        // First, process control flow (@if, @foreach)
        var processed = ProcessControlFlow(composed, context);

        // Then, resolve variables
        var result = ResolveVariables(processed, context);

        _logger.LogTrace("Render complete, output length: {Length}", result.Length);
        return result;
    }

    private static readonly System.Text.RegularExpressions.Regex AsyncServicePattern = new(
        @"@service\(",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Renders a template that may invoke asynchronous services. Every
    /// <c>@service("name", expression)</c> call is awaited in source order, and the resolved
    /// values then pass through the normal renderer.
    ///
    /// The order is sequential by contract, so a later call cannot run before an earlier one
    /// completes. A service that throws becomes a <see cref="TemplateServiceException"/> naming
    /// only the service, and a cancelled token surfaces as <see cref="OperationCanceledException"/>
    /// before any further service is invoked. Calling the synchronous <see cref="Render"/> with
    /// an async service in the template is an explicit refusal rather than a blocking wait.
    /// </summary>
    public async Task<string> RenderAsync(
        string template,
        ToolContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(template)) return template;

        var position = 0;
        var builder = new System.Text.StringBuilder(template.Length);
        while (true)
        {
            var start = template.IndexOf("@service(", position, StringComparison.Ordinal);
            if (start < 0)
            {
                builder.Append(template, position, template.Length - position);
                break;
            }

            builder.Append(template, position, start - position);

            var open = start + "@service".Length;
            var end = FindClosingParenthesis(template, open);
            if (end < 0)
            {
                throw new InvalidOperationException("An @service call is missing its closing parenthesis.");
            }

            var arguments = template[(open + 1)..end];
            var comma = arguments.IndexOf(',');
            if (comma < 0)
            {
                throw new InvalidOperationException("@service requires a service name and an argument expression.");
            }

            var name = arguments[..comma].Trim().Trim('"', '\'');
            var expression = arguments[(comma + 1)..].Trim();

            if (!context.TryGetAsyncService(name, out var service) || service is null)
            {
                throw new InvalidOperationException($"Asynchronous service '{name}' is not registered.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var input = _resolver.ResolveExpression(expression, context);
            object? value;
            try
            {
                value = await service(input, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception error)
            {
                throw new TemplateServiceException(name, error);
            }

            builder.Append(value?.ToString() ?? string.Empty);
            position = end + 1;
        }

        return RenderInternal(builder.ToString(), context, 0, null, Array.Empty<string>());
    }

    private static int FindClosingParenthesis(string source, int openIndex)
    {
        var depth = 0;
        for (var index = openIndex; index < source.Length; index++)
        {
            if (source[index] == '(') depth++;
            else if (source[index] == ')')
            {
                depth--;
                if (depth == 0) return index;
            }
        }

        return -1;
    }
    public string ResolveVariables(string input, ToolContext context)
    {
        return _resolver.ResolveString(input, context);
    }

    /// <summary>
    /// Registers a named fragment that a template can pull in with <c>@include("name")</c>.
    /// Fragments are in-memory template source owned by this engine instance: there is no
    /// filesystem, network or package resolution, and nothing is shared between engines.
    /// The name is compared ordinally.
    /// </summary>
    public void RegisterFragment(string name, string source)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A fragment name must not be null, empty or whitespace.", nameof(name));
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        _fragments[name] = source;
    }

    /// <summary>Removes a registered fragment. Removing a missing name is not an error.</summary>
    public bool UnregisterFragment(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A fragment name must not be null, empty or whitespace.", nameof(name));

        return _fragments.TryRemove(name, out _);
    }

    /// <summary>The fragments registered on this engine, exposed read-only.</summary>
    public IReadOnlyDictionary<string, string> Fragments => _fragments;

    private const int MacroRecursionLimit = 32;

    private static readonly System.Text.RegularExpressions.Regex MacroDefinitionPattern = new(
        @"@macro\s+(?<name>[A-Za-z_]\w*)\s*\((?<params>[^)]*)\)\s*\{",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex MacroCallPattern = new(
        @"@(?<name>[A-Za-z_]\w*)\s*\((?<args>[^)]*)\)",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly HashSet<string> DirectiveKeywords = new(StringComparer.Ordinal)
    {
        "if", "else", "elseif", "foreach", "include", "macro", "end", "endif", "model",
    };

    /// <summary>A macro declared in the template source: its parameter names and body.</summary>
    private sealed record MacroDefinition(string Name, string[] Parameters, string Body);

    /// <summary>
    /// Removes <c>@macro name(p1, p2) { body }</c> definitions from the source and expands
    /// <c>@name(args)</c> calls into the rendered body.
    ///
    /// The body is the balanced brace block after the parameter list, so it may contain any
    /// renderer construct. A call renders its body with the arguments bound as context values
    /// in a child context, which is why two calls with different arguments cannot share state.
    /// Arity mismatches and unknown names are named refusals; expansion is cut off at
    /// <see cref="MacroRecursionLimit"/> so a self-invoking macro terminates instead of
    /// overflowing the stack. A call is only treated as a macro call when the same template
    /// declares at least one macro, so text that merely looks like <c>@name(...)</c> is left
    /// alone.
    /// </summary>
    private string ExpandMacros(string source, ToolContext context, int depth, IReadOnlyDictionary<string, MacroDefinition>? inherited, IReadOnlyList<string> chain)
    {
        var definitions = new Dictionary<string, MacroDefinition>(StringComparer.Ordinal);
        var remaining = RemoveMacroDefinitions(source, definitions);
        if (inherited is not null)
        {
            foreach (var pair in inherited) definitions.TryAdd(pair.Key, pair.Value);
        }
        if (definitions.Count == 0) return remaining;

        return MacroCallPattern.Replace(remaining, match =>
        {
            var name = match.Groups["name"].Value;
            if (DirectiveKeywords.Contains(name)) return match.Value;
            if (!definitions.TryGetValue(name, out var macro))
            {
                throw new InvalidOperationException($"Macro '{name}' is not defined in this template.");
            }

            var arguments = SplitArguments(match.Groups["args"].Value);
            if (arguments.Count != macro.Parameters.Length)
            {
                throw new InvalidOperationException(
                    $"Macro '{name}' expects {macro.Parameters.Length} argument(s) but received {arguments.Count}.");
            }

            // The chain records the macros currently expanding, in order, so a repeat is a
            // named cycle naming the whole path rather than a bare depth-limit message.
            if (chain.Contains(name, StringComparer.Ordinal))
            {
                var cycle = string.Join(" -> ", chain.Concat([name]));
                throw new InvalidOperationException($"Macro recursion cycle detected: {cycle}");
            }

            var child = context.CreateChild($"macro:{name}");
            for (var index = 0; index < arguments.Count; index++)
            {
                child.Set(macro.Parameters[index], EvaluateArgument(arguments[index], context));
            }

            var deeper = new List<string>(chain) { name };
            return RenderInternal(macro.Body, child, depth + 1, definitions, deeper);
        });
    }

    private string RemoveMacroDefinitions(string source, Dictionary<string, MacroDefinition> definitions)
    {
        var output = new System.Text.StringBuilder(source.Length);
        var index = 0;
        while (index < source.Length)
        {
            var match = MacroDefinitionPattern.Match(source, index);
            if (!match.Success)
            {
                output.Append(source, index, source.Length - index);
                break;
            }

            output.Append(source, index, match.Index - index);

            var bodyStart = match.Index + match.Length;
            var depth = 1;
            var cursor = bodyStart;
            while (cursor < source.Length && depth > 0)
            {
                if (source[cursor] == '{') depth++;
                else if (source[cursor] == '}') depth--;
                cursor++;
            }

            if (depth != 0)
            {
                throw new InvalidOperationException(
                    $"Macro '{match.Groups["name"].Value}' has an unbalanced body.");
            }

            var parameters = match.Groups["params"].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries).Select(part => part.Trim()).ToArray();
            var name = match.Groups["name"].Value;
            definitions[name] = new MacroDefinition(name, parameters, source[bodyStart..(cursor - 1)]);
            index = cursor;
        }

        return output.ToString();
    }

    private static List<string> SplitArguments(string arguments) =>
        arguments.Trim().Length == 0
            ? []
            : arguments.Split(',').Select(part => part.Trim()).ToList();

    private object? EvaluateArgument(string expression, ToolContext context)
    {
        var trimmed = expression.Trim();
        if (trimmed.Length >= 2
            && ((trimmed[0] == '"' && trimmed[^1] == '"') || (trimmed[0] == '\'' && trimmed[^1] == '\'')))
        {
            return trimmed[1..^1];
        }

        return _resolver.ResolveExpression(trimmed, context);
    }

    /// <summary>
    /// Replaces every <c>@include("name")</c> with the named fragment's source, recursively.
    /// A fragment renders with the caller's own context, so values passed to one render are
    /// visible to its fragments and never leak into another render. A missing name or a cycle
    /// fails before any output is produced, so a caller never receives partial text.
    /// </summary>
    private string ExpandIncludes(string source, IReadOnlyList<string> nesting)
    {
        return IncludePattern.Replace(source, match =>
        {
            var name = match.Groups["name"].Value;

            if (nesting.Contains(name))
            {
                var path = string.Join(" -> ", nesting.Append(name));
                throw new InvalidOperationException($"Include cycle detected: {path}");
            }

            if (!_fragments.TryGetValue(name, out var fragment))
            {
                throw new InvalidOperationException($"Include '{name}' was not registered on this engine.");
            }

            var deeper = new List<string>(nesting) { name };
            return ExpandIncludes(fragment, deeper);
        });
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
