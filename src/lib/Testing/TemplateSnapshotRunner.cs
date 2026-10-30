namespace LowlandTech.TinyTools.Testing;

/// <summary>One snapshot case on disk: a template, an optional model and an approved output file.</summary>
public sealed record SnapshotCase(string Name, string TemplatePath, string? ModelPath, string ApprovedPath);

/// <summary>
/// What a snapshot run observed for one case. A failure carries the first differing line and a
/// bounded diff so a reviewer sees the change without scrolling a generated file.
/// </summary>
public sealed record SnapshotResult(
    SnapshotCase Case,
    bool Ok,
    string Message,
    int? FirstDifferingLine = null,
    string Diff = "");

/// <summary>
/// Runs template snapshot cases: render each template, compare it with the approved file beside it,
/// and rewrite approvals only when the caller explicitly asks.
///
/// The layout is deliberately plain so an approval is a normal review artifact:
/// <c>name.tmpl</c>, an optional <c>name.model.json</c>, and <c>name.approved.txt</c>.
/// </summary>
public sealed class TemplateSnapshotRunner(TinyTemplateEngine? engine = null)
{
    private const int DiffLineLimit = 40;

    private readonly TinyTemplateEngine _engine = engine ?? new TinyTemplateEngine();

    /// <summary>Finds the cases in a directory, ordered by name so a run is deterministic.</summary>
    public static IReadOnlyList<SnapshotCase> Discover(string directory)
    {
        if (!Directory.Exists(directory)) return [];

        return Directory
            .EnumerateFiles(directory, "*.tmpl")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path =>
            {
                var name = Path.GetFileNameWithoutExtension(path);
                var model = Path.Combine(directory, name + ".model.json");
                return new SnapshotCase(
                    name,
                    path,
                    File.Exists(model) ? model : null,
                    Path.Combine(directory, name + ".approved.txt"));
            })
            .ToList();
    }

    /// <summary>Runs every case; with <paramref name="approve"/>, writes the approved files it touched.</summary>
    public IReadOnlyList<SnapshotResult> Run(IEnumerable<SnapshotCase> cases, bool approve = false) =>
        cases.Select(item => RunCase(item, approve)).ToList();

    private SnapshotResult RunCase(SnapshotCase item, bool approve)
    {
        string template;
        try
        {
            template = File.ReadAllText(item.TemplatePath);
        }
        catch (IOException error)
        {
            return new(item, false, $"Could not read '{item.TemplatePath}': {error.Message}");
        }

        var validation = _engine.ValidateSyntax(template);
        if (!validation.IsValid)
        {
            var first = validation.Diagnostics[0];
            return new(item, false, $"Template '{item.TemplatePath}' is invalid: {first.Code} at line {first.Line}, column {first.Column}: {first.Message}");
        }

        var context = new ExecutionContext();
        if (item.ModelPath is not null)
        {
            try
            {
                context.Model = System.Text.Json.JsonSerializer.Deserialize<object>(File.ReadAllText(item.ModelPath));
            }
            catch (System.Text.Json.JsonException error)
            {
                return new(item, false, $"Model '{item.ModelPath}' is not valid JSON: {error.Message}");
            }
        }

        var rendered = _engine.Render(template, context);

        if (!File.Exists(item.ApprovedPath))
        {
            if (approve)
            {
                File.WriteAllText(item.ApprovedPath, rendered);
                return new(item, true, $"Approved '{item.ApprovedPath}'.");
            }

            return new(item, false, $"No approved file at '{item.ApprovedPath}'.");
        }

        var approved = File.ReadAllText(item.ApprovedPath);
        if (string.Equals(approved, rendered, StringComparison.Ordinal))
        {
            return new(item, true, $"'{item.Name}' matches its approval.");
        }

        if (approve)
        {
            File.WriteAllText(item.ApprovedPath, rendered);
            return new(item, true, $"Updated '{item.ApprovedPath}'.");
        }

        var (line, diff) = DescribeDifference(approved, rendered);
        return new(item, false, $"'{item.Name}' changed at line {line}.", line, diff);
    }

    /// <summary>The first differing one-based line plus a bounded diff of the affected region.</summary>
    private static (int Line, string Diff) DescribeDifference(string approved, string rendered)
    {
        var expected = approved.Split('\n');
        var actual = rendered.Split('\n');
        var first = 1;
        while (first <= expected.Length && first <= actual.Length
               && string.Equals(expected[first - 1], actual[first - 1], StringComparison.Ordinal))
        {
            first++;
        }

        var lines = new List<string>();
        for (var index = first - 1; index < Math.Min(expected.Length, actual.Length) && lines.Count < DiffLineLimit; index++)
        {
            if (string.Equals(expected[index], actual[index], StringComparison.Ordinal)) continue;
            lines.Add($"-{index + 1}: {expected[index]}");
            lines.Add($"+{index + 1}: {actual[index]}");
        }

        return (Math.Min(first, Math.Min(expected.Length, actual.Length) + 1), string.Join("\n", lines));
    }
}
