using System.Text.RegularExpressions;

namespace LowlandTech.TinyTools.Tests.SPEC3308.Documentation.US01.PublicApi;

/// <summary>
/// The readme is the package's front page, so its samples are behaviour: the
/// scenario runs what it documents and compares its helper table with the
/// helpers the engine registers.
/// </summary>
[Trait(Spec.SPEC, "3308")]
[Trait(Spec.SC, "01")]
[UserStory("01", "The documented API is the API a consumer gets")]
public sealed class SC01_DocumentedSamplesCompile : IAsyncLifetime
{
    private string _renderOutput = string.Empty;
    private int _missingHelpers;
    private int _documentsNamingToolContext;
    private IReadOnlyList<string> _documented = [];

    [Given("the repository documentation and the engine's registered helpers")]
    private void Given()
    {
        _documentsNamingToolContext = Documents()
            .Count(file => File.ReadAllText(file).Contains("ToolContext", StringComparison.Ordinal));
        _documented = DocumentedHelpers().ToList();
    }

    [When("the documented samples run and the helper table is compared with the registry")]
    private void When()
    {
        var context = new ExecutionContext();
        context.Set("FirstName", "John");
        context.Set("LastName", "Smith");
        _renderOutput = new TinyTemplateEngine().Render("Hello ${FirstName} ${LastName}!", context);

        var registered = TemplateHelpers.Helpers.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _missingHelpers = _documented.Count(helper => !registered.Contains(helper));
    }

    public ValueTask InitializeAsync()
    {
        Given();
        When();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Then("the tool context mentions should have count 0")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_NoDocumentMentionsToolContext() => _documentsNamingToolContext.Should().Be(0);

    [Then("the quick start should contain Hello John Smith")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_QuickStartRendersAsDocumented() => _renderOutput.Should().Contain("Hello John Smith");

    [Then("the missing helpers should have count 0")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_EveryDocumentedHelperIsRegistered() => _missingHelpers.Should().Be(0);

    [Then("the readme should contain kebabcase")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_TheThreeAddedHelpersAreDocumented() => _documented.Should().Contain(["kebabcase", "snakecase", "titlecase"]);

    private static IEnumerable<string> Documents()
    {
        foreach (var name in new[] { "readme.md", Path.Combine("src", "README.md"), "CONTRIBUTING.md" })
        {
            var path = Path.Combine(RepositoryRoot(), name);
            if (File.Exists(path)) yield return path;
        }
    }

    private static HashSet<string> DocumentedHelpers() =>
        Regex.Matches(File.ReadAllText(Path.Combine(RepositoryRoot(), "readme.md")), @"^\|\s*`([a-z]+)(?::[^`]*)?`\s*\|", RegexOptions.Multiline)
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "tinytools.slnx")))
            directory = directory.Parent;

        return directory?.FullName
               ?? throw new InvalidOperationException("The repository root is not reachable from the test output directory.");
    }
}
