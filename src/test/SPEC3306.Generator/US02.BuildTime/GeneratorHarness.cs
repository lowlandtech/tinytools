using System.Collections.Immutable;
using System.Text;
using LowlandTech.TinyTools.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace LowlandTech.TinyTools.Tests.SPEC3306.Generator.US02.BuildTime;

/// <summary>
/// The consumer fixture the refinement requires: it compiles a consumer with the generator
/// attached, so the assertions inspect a real compilation rather than library behaviour.
/// </summary>
internal static class GeneratorHarness
{
    /// <summary>The consumer source the fixture compiles: a model plus a static entry point.</summary>
    internal const string ConsumerSource = """
        namespace Consumer;
        public sealed class Model { public string Name { get; set; } = ""; }
        public static class Entry { public static string Run() => Templates.GreetingRenderer.Render(new Model { Name = "Ada" }); }
        """;

    internal sealed record Consumer(
        Compilation Output,
        ImmutableArray<Diagnostic> Diagnostics,
        IReadOnlyList<(string HintName, string Text)> Generated);

    internal static Consumer Build(
        (string Path, string Text, Dictionary<string, string> Metadata)[] templates,
        string consumerSource,
        string assemblyName = "Consumer")
    {
        var generator = new TemplateRendererGenerator();
        var additional = templates
            .Select(template => (AdditionalText)new InMemoryAdditionalText(template.Path, template.Text))
            .ToArray();

        var driver = CSharpGeneratorDriver.Create(
            [generator.AsSourceGenerator()],
            additionalTexts: additional,
            optionsProvider: new MetadataOptionsProvider(templates.ToDictionary(t => t.Path, t => t.Metadata, StringComparer.Ordinal)),
            parseOptions: new CSharpParseOptions(LanguageVersion.Latest));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(consumerSource)],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var updated = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        var generated = updated.GetRunResult().Results
            .SelectMany(result => result.GeneratedSources)
            .Select(source => (source.HintName, source.SourceText.ToString()))
            .ToList();

        return new Consumer(output, diagnostics, generated);
    }

    /// <summary>Emits the consumer, loads it and invokes a static entry point by name.</summary>
    internal static string Invoke(Compilation compilation, string typeName, string methodName)
    {
        using var stream = new MemoryStream();
        var emitted = compilation.Emit(stream);
        if (!emitted.Success)
        {
            throw new InvalidOperationException(string.Join("; ", emitted.Diagnostics.Select(d => d.ToString())));
        }

        var assembly = System.Reflection.Assembly.Load(stream.ToArray());
        var method = assembly.GetType(typeName, throwOnError: true)!.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!;
        return (string)method.Invoke(null, null)!;
    }

    internal static IReadOnlyList<MetadataReference> References()
    {
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal) || Path.GetFileName(path) is "mscorlib.dll" or "netstandard.dll");

        var locations = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
        locations.Add(typeof(TinyTemplateEngine).Assembly.Location);
        // The engine constructor takes an ILoggerFactory, so the consumer compilation needs
        // the logging abstractions as well.
        locations.Add(typeof(Microsoft.Extensions.Logging.ILoggerFactory).Assembly.Location);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location)) continue;
            locations.Add(assembly.Location);
        }

        return locations.Select(location => (MetadataReference)MetadataReference.CreateFromFile(location)).ToList();
    }

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(text, Encoding.UTF8);
    }

    private sealed class MetadataOptionsProvider(Dictionary<string, Dictionary<string, string>> metadata)
        : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(new Dictionary<string, string>());

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new Options(new Dictionary<string, string>());

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
            new Options(metadata.TryGetValue(textFile.Path, out var values) ? values : new Dictionary<string, string>());
    }

    private sealed class Options(Dictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
        {
            if (values.TryGetValue(key, out var found))
            {
                value = found;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
