namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "13")]
[UserStory("01", "String interpolation processes file names and contents")]
public class WhenInterpolatingFilesTest : TinyToolsScenario<List<string>>
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
            { "folderName","src" },
        };
        Sut.Interpolate(replacements, true)
           .Select(file => Path.GetRelativePath(_folder, file))
           .Count(file => file.Split(Path.DirectorySeparatorChar).Contains("src"))
           .Should()
           .Be(1);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Interpolate File Name")]
    [Fact]
    public void ItShouldInterpolateFileName()
    {
        ArrangeAndAct();
        var replacements = new Dictionary<string, string>()
        {
            { "fileName","codefile1" },
        };
        Sut.Interpolate(replacements, true)
            .Count(file => file.Contains("codefile1"))
            .Should()
            .Be(1);
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Interpolate File Extension")]
    [Fact]
    public void ItShouldInterpolateFileExtension()
    {
        ArrangeAndAct();
        var replacements = new Dictionary<string, string>()
        {
            { "extension","cs" },
        };
        Sut.Interpolate(replacements, true)
            .Count(file => Path.GetExtension(file) == ".cs")
            .Should()
            .Be(1);
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Interpolate File Text")]
    [Fact]
    public void ItShouldInterpolateFileText()
    {
        ArrangeAndAct();
        var replacements = new Dictionary<string, string>()
        {
            { "fileName","updated" },
            { "fileText","Hello world"}
        };

        var result = Sut.Interpolate(replacements, hasTags: true, isFile: true)
            .Single(file => file.Contains("updated"));

        result.Should().NotBeNullOrEmpty();

        File.ReadAllText(result)
            .Should().Be("Hello world");
    }
}
