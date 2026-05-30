namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "14")]
[UserStory("01", "String interpolation processes files without tags")]
public class WhenInterpolatingFilesUntaggedTest : TinyToolsScenario<List<string>>
{
    private string _root = null!;
    private string _folder = null!;

    protected override void SetupData()
    {
        var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
        var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
        _root = Path.GetDirectoryName(codeBasePath)!;
        _folder = Path.Combine(_root, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        ZipFile.ExtractToDirectory(Path.Combine(_root, "files.zip"), _folder);
    }

    protected override List<string> For()
    {
        return Directory.GetFiles(_folder, "*.*", SearchOption.AllDirectories).ToList();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        Sut.Count.Should().Be(8);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Interpolate File Folder")]
    [Fact]
    public void ItShouldInterpolateFileFolder()
    {
        ArrangeAndAct();
        var replacements = new Dictionary<string, string>()
        {
            { "subfolder1","src" },
            { "subfolder2","lowlandtech" }
        };
        Sut.Interpolate(replacements)
           .Count(file => file.Contains(Path.Combine("src", "lowlandtech")))
           .Should()
           .Be(2);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Interpolate File Name")]
    [Fact]
    public void ItShouldInterpolateFileName()
    {
        ArrangeAndAct();
        var replacements = new Dictionary<string, string>()
        {
            { "file1","codefile1" },
            { "file2","codefile2" }
        };
        Sut.Interpolate(replacements)
            .Count(file => file.Contains("codefile1"))
            .Should()
            .Be(3);

        Sut.Interpolate(replacements)
            .Count(file => file.Contains("codefile2"))
            .Should()
            .Be(3);
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Interpolate File Extension")]
    [Fact]
    public void ItShouldInterpolateFileExtension()
    {
        ArrangeAndAct();
        var replacements = new Dictionary<string, string>()
        {
            { ".txt",".cs" },
        };
        Sut.Interpolate(replacements)
            .Count(file => file.Contains(".cs"))
            .Should()
            .Be(7);
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Interpolate File Text")]
    [Fact]
    public void ItShouldInterpolateFileText()
    {
        ArrangeAndAct();
        var replacements = new Dictionary<string, string>()
        {
            { "file3","updated" },
            { "fileText","Hello world"}
        };

        var result = Sut.Interpolate(replacements, hasTags: false, isFile: true)
            .Single(file => file.Contains("updated"));

        result.Should().NotBeNullOrEmpty();

        File.ReadAllText(result)
            .Should().Be("Hello world");
    }

    public override void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        base.Dispose();
    }
}
