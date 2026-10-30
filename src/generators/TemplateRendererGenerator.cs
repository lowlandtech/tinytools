using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace LowlandTech.TinyTools.Generators;

/// <summary>The stable diagnostic identity of this generator.</summary>
internal static class Diagnostics
{
    private const string Category = "LowlandTech.TinyTools.Generator";

    internal static readonly DiagnosticDescriptor MalformedTemplate = new(
        "TTG001",
        "Template cannot be generated",
        "Template '{0}' cannot be generated at line {1}, column {2}: {3}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor MissingModel = new(
        "TTG002",
        "Template has no model type",
        "Template '{0}' has no TinyToolsModel metadata, so no typed renderer can be generated",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor UnknownMember = new(
        "TTG005",
        "Template names a member the model does not have",
        "Template '{0}' uses '{1}', which is not a member of '{2}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor NullableSegment = new(
        "TTG006",
        "Template dereferences a nullable member",
        "Template '{0}' dereferences nullable '{1}' of '{2}' without a value",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor DuplicateName = new(
        "TTG003",
        "Two templates generate the same renderer",
        "Templates '{1}' and '{2}' both generate renderer '{0}'; set TinyToolsOutputName on one of them",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

/// <summary>A one-based location for a syntax problem found in template text.</summary>
internal readonly struct TemplateError
{
    internal TemplateError(int offset, int line, int column, string message)
    {
        Offset = offset; Line = line; Column = column; Message = message;
    }

    internal int Offset { get; }
    internal int Line { get; }
    internal int Column { get; }
    internal string Message { get; }
}

/// <summary>
/// The small syntax check the generator needs before it emits code: an unclosed
/// <c>@if</c>/<c>@foreach</c> block or an unterminated <c>${</c> expression cannot produce a
/// working renderer, so it becomes TTG001 with a real line and column.
/// </summary>
internal static class TemplateSyntax
{
    private static readonly string[] BlockOpeners = ["@if (", "@if(", "@foreach (", "@foreach("];

    internal static TemplateError? FindError(string template)
    {
        var depth = 0;
        var line = 1;
        var column = 1;
        var inExpression = false;

        for (var index = 0; index < template.Length; index++)
        {
            var character = template[index];

            if (character == '\n')
            {
                line++;
                column = 1;
                continue;
            }

            column++;

            // A ${...} expression owns its closing brace, so it must not affect block depth.
            if (!inExpression && character == '$' && index + 1 < template.Length && template[index + 1] == '{')
            {
                inExpression = true;
                index++;
                column++;
                continue;
            }

            if (inExpression)
            {
                if (character == '}') inExpression = false;
                continue;
            }

            if (Opens(template, index))
            {
                depth++;
                continue;
            }

            if (character == '}')
            {
                depth--;
                if (depth < 0) return new(index, line, column, "unexpected closing brace");
            }
        }

        if (inExpression) return new(Math.Max(0, template.Length - 1), line, column, "unterminated variable expression");
        if (depth > 0) return new(Math.Max(0, template.Length - 1), line, column, "unclosed block");
        return null;
    }

    private static bool Opens(string template, int index)
    {
        foreach (var opener in BlockOpeners)
        {
            if (string.CompareOrdinal(template, index, opener, 0, opener.Length) == 0) return true;
        }

        return false;
    }
}

/// <summary>
/// Generates a strongly typed renderer for every <c>.tmpl</c> additional file, so a consumer
/// calls <c>Render(model)</c> and never parses or compiles a template at run time.
///
/// Item metadata is the whole contract:
/// <c>TinyToolsModel</c> (fully qualified model type, required),
/// <c>TinyToolsNamespace</c> (default: <c>Templates</c>) and
/// <c>TinyToolsOutputName</c> (default: the file name without its extension).
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class TemplateRendererGenerator : IIncrementalGenerator
{
    private const string ModelMetadata = "TinyToolsModel";
    private const string NamespaceMetadata = "TinyToolsNamespace";
    private const string OutputMetadata = "TinyToolsOutputName";
    private const string LenientMetadata = "TinyToolsLenient";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var templates = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".tmpl", StringComparison.OrdinalIgnoreCase))
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (pair, cancellationToken) => Describe(
                pair.Left.Path,
                pair.Left.GetText(cancellationToken)?.ToString() ?? string.Empty,
                pair.Right.GetOptions(pair.Left)));

        // The compilation is combined in so member paths can be checked against the declared
        // model type instead of being trusted.
        context.RegisterSourceOutput(
            templates.Collect().Combine(context.CompilationProvider),
            static (production, pair) => Emit(production, pair.Left, pair.Right));
    }

    private static TemplateCandidate Describe(string path, string text, AnalyzerConfigOptions options)
    {
        options.TryGetValue(ModelMetadata, out var model);
        options.TryGetValue(NamespaceMetadata, out var @namespace);
        options.TryGetValue(OutputMetadata, out var output);
        options.TryGetValue(LenientMetadata, out var lenient);

        var name = Sanitize(string.IsNullOrWhiteSpace(output)
            ? System.IO.Path.GetFileNameWithoutExtension(path)
            : output!);

        return new TemplateCandidate(
            path,
            text,
            model,
            string.IsNullOrWhiteSpace(@namespace) ? "Templates" : @namespace!,
            name,
            string.Equals(lenient, "true", StringComparison.OrdinalIgnoreCase));
    }

    private static void Emit(SourceProductionContext production, IReadOnlyList<TemplateCandidate> inputs, Compilation compilation)
    {
        var seen = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var candidate in inputs)
        {
            if (string.IsNullOrWhiteSpace(candidate.Model))
            {
                production.ReportDiagnostic(Diagnostic.Create(Diagnostics.MissingModel, candidate.Location(), candidate.Path));
                continue;
            }

            if (seen.TryGetValue(candidate.Name, out var other))
            {
                production.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.DuplicateName, candidate.Location(), candidate.Name, other, candidate.Path));
                continue;
            }

            seen[candidate.Name] = candidate.Path;

            var error = TemplateSyntax.FindError(candidate.Text);
            if (error is not null)
            {
                production.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.MalformedTemplate,
                    candidate.Location(error.Value.Offset),
                    candidate.Path,
                    error.Value.Line,
                    error.Value.Column,
                    error.Value.Message));
                continue;
            }

            var memberError = candidate.Lenient ? null : MemberValidation.Validate(candidate, compilation);
            if (memberError is not null)
            {
                production.ReportDiagnostic(Diagnostic.Create(memberError.Value.Descriptor, candidate.Location(memberError.Value.Offset), memberError.Value.Arguments));
                continue;
            }

            production.AddSource($"{candidate.Name}.Renderer.g.cs", SourceText.From(Render(candidate), Encoding.UTF8));
        }
    }

    private static string Render(TemplateCandidate candidate)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.Append("namespace ").AppendLine(candidate.Namespace);
        builder.AppendLine("{");
        builder.Append("    public static class ").Append(candidate.Name).AppendLine("Renderer");
        builder.AppendLine("    {");
        builder.Append("        private const string Template = @\"").Append(candidate.Text.Replace("\"", "\"\"")).AppendLine("\";");
        builder.AppendLine();
        builder.Append("        public static string Render(").Append(candidate.Model).AppendLine(" model)");
        builder.AppendLine("        {");
        builder.AppendLine("            var context = new LowlandTech.TinyTools.ExecutionContext { Model = model };");
        builder.AppendLine("            return new LowlandTech.TinyTools.TinyTemplateEngine().Render(Template, context);");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string Sanitize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        var name = builder.ToString();
        if (name.Length == 0) return "Template";
        if (char.IsDigit(name[0])) name = "T" + name;

        // A generated renderer is public API, so its name is Pascal-cased.
        return char.ToUpperInvariant(name[0]) + name.Substring(1);
    }

    /// <summary>
    /// Checks each <c>${Context.X...}</c> path against the declared model type, so a typo fails the
    /// build instead of rendering an empty string at run time.
    /// </summary>
    private static class MemberValidation
    {
        private static readonly System.Text.RegularExpressions.Regex Token = new(
            @"\$\{([^}]+)\}",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        internal static (DiagnosticDescriptor Descriptor, int Offset, object[] Arguments)? Validate(
            TemplateCandidate candidate,
            Compilation compilation)
        {
            var model = compilation.GetTypeByMetadataName(candidate.Model!);
            if (model is null) return null;

            foreach (System.Text.RegularExpressions.Match match in Token.Matches(candidate.Text))
            {
                var offset = match.Index;
                var path = match.Groups[1].Value.Split('|')[0].Trim();
                if (path.StartsWith("Context.", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring("Context.".Length);
                }

                // The first segment names the context key that holds the model, so member
                // checking starts at the type itself rather than looking for a "Model" member.
                if (path.StartsWith("Model.", StringComparison.Ordinal))
                {
                    path = path.Substring("Model.".Length);
                }

                if (path.Length == 0 || path[0] == '"' || path[0] == '\''
                    || path.Contains("??", StringComparison.Ordinal) || path.IndexOf('?') >= 0 || path.IndexOf('(') >= 0)
                {
                    continue;
                }

                var current = (ITypeSymbol?)model;
                for (var index = 0; index < path.Split('.').Length; index++)
                {
                    var segment = path.Split('.')[index];
                    if (current is null) break;

                    var member = FindMember(current, segment);
                    if (member is null)
                    {
                        return (Diagnostics.UnknownMember, offset, [candidate.Path, segment, candidate.Model!]);
                    }

                    current = member.Value.Type;
                    var more = index < path.Split('.').Length - 1;
                    if (more && current.NullableAnnotation == NullableAnnotation.Annotated)
                    {
                        return (Diagnostics.NullableSegment, offset, [candidate.Path, segment, candidate.Model!]);
                    }
                }
            }

            return null;
        }

        private static (ITypeSymbol Type, ISymbol Symbol)? FindMember(ITypeSymbol type, string name)
        {
            foreach (var member in type.GetMembers(name))
            {
                if (member is IPropertySymbol { Type: var property }) return (property, member);
                if (member is IFieldSymbol { Type: var field }) return (field, member);
            }

            return null;
        }
    }

    private sealed class TemplateCandidate
    {
        internal TemplateCandidate(string path, string text, string? model, string @namespace, string name, bool lenient)
        {
            Path = path; Text = text; Model = model; Namespace = @namespace; Name = name; Lenient = lenient;
        }

        internal bool Lenient { get; }

        internal string Path { get; }
        internal string Text { get; }
        internal string? Model { get; }
        internal string Namespace { get; }
        internal string Name { get; }

        internal global::Microsoft.CodeAnalysis.Location Location(int offset = 0)
        {
            // No syntax tree exists for an additional file, so the location names the path and
            // carries the offset span a compiler will still print as file(line,column).
            var start = offset < Text.Length ? offset : 0;
            return global::Microsoft.CodeAnalysis.Location.Create(
                Path,
                new TextSpan(start, start < Text.Length ? 1 : 0),
                new LinePositionSpan(new LinePosition(0, start), new LinePosition(0, start + 1)));
        }
    }
}