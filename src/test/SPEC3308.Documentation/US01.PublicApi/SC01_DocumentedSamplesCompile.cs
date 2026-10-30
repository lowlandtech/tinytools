using System.Reflection;
using System.Text.RegularExpressions;

namespace LowlandTech.TinyTools.Tests.SPEC3308.Documentation.US01.PublicApi;

/// <summary>
/// The readme is the package's front page, so its samples are behaviour: these
/// facts run what it documents and compare its helper table with the helpers the
/// engine registers.
/// </summary>
[Trait(Spec.SPEC, "3308")]
[Trait(Spec.SC, "01")]
[UserStory("01", "The documented API is the API a consumer gets")]
public class WhenReadingTheDocumentedApi
{
    [Trait(Spec.UAC, "01")]
    [Fact]
    public void ItShouldNameOnlyPublicContextTypesInTheDocumentation()
    {
        var engineAssembly = typeof(TinyTemplateEngine).Assembly;
        var publicTypes = engineAssembly.GetExportedTypes().Select(type => type.Name).ToList();

        publicTypes.Should().Contain("ExecutionContext");
        publicTypes.Should().NotContain("ToolContext");

        foreach (var file in DocumentedFiles())
        {
            File.ReadAllText(file).Should().NotContain("ToolContext", because: $"{Path.GetFileName(file)} must name a type a consumer can see");
        }
    }

    [Trait(Spec.UAC, "01")]
    [Fact]
    public void ItShouldRenderTheQuickStartSampleAsDocumented()
    {
        var engine = new TinyTemplateEngine();
        var context = new ExecutionContext();
        context.Set("FirstName", "John");
        context.Set("LastName", "Smith");

        var result = engine.Render("Hello ${FirstName} ${LastName}!", context);

        result.Should().Be("Hello John Smith!");
    }

    [Trait(Spec.UAC, "03")]
    [Fact]
    public void ItShouldDocumentEveryRegisteredHelper()
    {
        var documented = DocumentedHelpers();
        var registered = TemplateHelpers.Helpers.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Every helper the readme offers must exist. The reverse direction — an
        // engine helper the readme omits — cannot be asserted while the registry
        // is a mutable process-wide dictionary that other specs add to (M5-002).
        documented.Should().BeSubsetOf(registered,
            because: "the readme must not offer a helper the engine does not register");

        documented.Should().Contain(["kebabcase", "snakecase", "titlecase"],
            because: "these three helpers existed without being documented");
    }

    private static IEnumerable<string> DocumentedFiles()
    {
        foreach (var name in new[] { "readme.md", Path.Combine("src", "README.md"), "CONTRIBUTING.md" })
        {
            var path = Path.Combine(RepositoryRoot(), name);
            if (File.Exists(path)) yield return path;
        }
    }

    private static HashSet<string> DocumentedHelpers()
    {
        var readme = File.ReadAllText(Path.Combine(RepositoryRoot(), "readme.md"));
        var helpers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Helper rows are the table lines whose first cell is a backticked name,
        // optionally with an argument after a colon.
        foreach (Match match in Regex.Matches(readme, @"^\|\s*`([a-z]+)(?::[^`]*)?`\s*\|", RegexOptions.Multiline))
            helpers.Add(match.Groups[1].Value);

        return helpers;
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "tinytools.slnx")))
            directory = directory.Parent;

        return directory?.FullName
               ?? throw new InvalidOperationException("The repository root is not reachable from the test output directory.");
    }
}
