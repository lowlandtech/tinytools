using System.Text.Json;

namespace LowlandTech.TinyTools.Packs;

/// <summary>One template declared by a pack manifest.</summary>
public sealed record TemplatePackEntry(string Name, string Path);

/// <summary>
/// A template pack manifest, read from <c>tinytools/pack.json</c> inside the package.
/// The pack carries template data only: an engine never loads code from it.
/// </summary>
public sealed record TemplatePackManifest(
    string Id,
    string Version,
    string Engine,
    IReadOnlyList<TemplatePackEntry> Templates)
{
    /// <summary>Stable identity for collision detection: id and version, ordinally compared.</summary>
    public string Identity => $"{Id}@{Version}";
}

/// <summary>A named refusal for a pack that must not be admitted.</summary>
public sealed record TemplatePackRefusal(string Reason, string Detail)
{
    /// <inheritdoc />
    public override string ToString() => $"{Reason}: {Detail}";
}

/// <summary>
/// Reads and admits template packs.
///
/// Contract: a pack is admitted only when its manifest parses, its engine range admits the
/// running engine, and every declared template path is a plain relative path to a declared
/// file with a template extension. Anything else - traversal, absolute paths, executable
/// content, undeclared files, a duplicate identity or a duplicate template name - is refused
/// by name before any content is rendered.
/// </summary>
public static class TemplatePackAdmission
{
    private static readonly string[] AllowedExtensions = [".tpl", ".template", ".txt", ".md", ".html", ".json", ".yml", ".yaml", ".cs"];
    private static readonly string[] RefusedExtensions = [".dll", ".exe", ".ps1", ".sh", ".bat", ".cmd", ".so", ".dylib"];

    /// <summary>Parses a manifest, or returns a refusal.</summary>
    public static TemplatePackManifest? TryRead(string json, out TemplatePackRefusal? refusal)
    {
        refusal = null;
        try
        {
            var manifest = JsonSerializer.Deserialize<TemplatePackManifest>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (manifest is null || string.IsNullOrWhiteSpace(manifest.Id) || string.IsNullOrWhiteSpace(manifest.Version))
            {
                refusal = new("invalid-manifest", "The manifest must declare an id and a version.");
                return null;
            }

            return manifest;
        }
        catch (JsonException error)
        {
            refusal = new("invalid-manifest", error.Message);
            return null;
        }
    }

    /// <summary>
    /// Admits a pack whose files are given as relative path to content, resolving engine
    /// compatibility and the identity collisions in <paramref name="admitted"/>.
    /// </summary>
    public static TemplatePackAdmissionResult Admit(
        TemplatePackManifest manifest,
        IReadOnlyDictionary<string, string> files,
        string engineVersion,
        IEnumerable<TemplatePackManifest> admitted)
    {
        if (manifest is null) throw new ArgumentNullException(nameof(manifest));
        if (files is null) throw new ArgumentNullException(nameof(files));

        if (!VersionRange.Admits(manifest.Engine, engineVersion))
        {
            return new(false, new("engine-incompatible", $"'{manifest.Engine}' does not admit engine {engineVersion}."), []);
        }

        var declared = new HashSet<string>(manifest.Templates.Select(entry => entry.Path), StringComparer.OrdinalIgnoreCase);
        foreach (var entry in manifest.Templates)
        {
            if (entry.Path.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(entry.Path) || entry.Path.StartsWith('/') || entry.Path.StartsWith('\\'))
            {
                return new(false, new("path-traversal", entry.Path), []);
            }

            if (!declared.Contains(entry.Path) || !files.ContainsKey(entry.Path))
            {
                return new(false, new("undeclared-template", entry.Path), []);
            }

            var extension = Path.GetExtension(entry.Path);
            if (RefusedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return new(false, new("executable-content", entry.Path), []);
            }

            if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return new(false, new("unsupported-extension", entry.Path), []);
            }
        }

        foreach (var path in files.Keys)
        {
            if (!declared.Contains(path))
            {
                return new(false, new("undeclared-file", path), []);
            }
        }

        foreach (var other in admitted)
        {
            if (string.Equals(other.Id, manifest.Id, StringComparison.OrdinalIgnoreCase))
            {
                return new(false, new("duplicate-identity", $"{other.Identity} and {manifest.Identity}"), []);
            }

            var clash = other.Templates
                .Select(entry => entry.Name)
                .Intersect(manifest.Templates.Select(entry => entry.Name), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (clash is not null)
            {
                return new(false, new("duplicate-template", $"{other.Identity} and {manifest.Identity} both declare '{clash}'"), []);
            }
        }

        var templates = manifest.Templates.Select(entry => new TemplatePackTemplate(entry.Name, files[entry.Path], manifest)).ToList();
        return new(true, null, templates);
    }
}

/// <summary>The outcome of admitting one pack: either its templates or a named refusal.</summary>
public sealed record TemplatePackAdmissionResult(
    bool Admitted,
    TemplatePackRefusal? Refusal,
    IReadOnlyList<TemplatePackTemplate> Templates);

/// <summary>A template admitted from a pack, with the provenance of the pack it came from.</summary>
public sealed record TemplatePackTemplate(string Name, string Content, TemplatePackManifest Pack);

/// <summary>
/// A minimal NuGet-style interval check: <c>[1.0,2.0)</c>, <c>(,2.0]</c> or a bare minimum
/// <c>2026.10.3</c> meaning "at least this version". Comparison is on dotted numeric parts.
/// </summary>
public static class VersionRange
{
    /// <summary>Whether <paramref name="range"/> admits <paramref name="version"/>.</summary>
    public static bool Admits(string? range, string version)
    {
        if (string.IsNullOrWhiteSpace(range)) return true;
        if (!Version.TryParse(version, out var candidate)) return false;

        var trimmed = range.Trim();
        if (!trimmed.Contains(','))
        {
            return Version.TryParse(trimmed.TrimStart('>', '='), out var minimum) && candidate >= minimum;
        }

        var inner = trimmed.Trim('[', ']', '(', ')');
        var bounds = inner.Split(',');
        var lower = bounds[0].Trim();
        var upper = bounds[1].Trim();
        var lowerInclusive = trimmed.StartsWith('[');
        var upperInclusive = trimmed.EndsWith(']');

        if (lower.Length > 0 && Version.TryParse(lower, out var low))
        {
            if (candidate < low || (!lowerInclusive && candidate == low)) return false;
        }

        if (upper.Length > 0 && Version.TryParse(upper, out var high))
        {
            if (candidate > high || (!upperInclusive && candidate == high)) return false;
        }

        return true;
    }
}
