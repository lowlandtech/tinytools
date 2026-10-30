namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "13")]
[UserStory("01", "String interpolation processes file names and contents")]
public sealed class SC13_InterpolatesFiles : TinyToolsScenario<List<string>>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private string _root = null!;
    private string _folder = null!;

    public SC13_InterpolatesFiles(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override void SetupData()
    {
        var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
        _root = Path.GetDirectoryName(Uri.UnescapeDataString(codeBaseUrl.AbsolutePath))!;
        _folder = Path.Combine(_root, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        ZipFile.ExtractToDirectory(Path.Combine(_root, "files.zip"), _folder);
    }

    protected override List<string> For() => Directory.GetFiles(_folder, "*.*", SearchOption.AllDirectories).ToList();

    [Given("an extracted file template folder holding eight files")]
    protected override void Given() => base.Given();

    [When("the file names and texts are interpolated once per case")]
    protected override void When()
    {
        base.When();

        useCase.Cases["FolderMatches"] = Sut.Interpolate(new Dictionary<string, string> { { "folderName", "src" } }, true)
            .Select(file => Path.GetRelativePath(_folder, file))
            .Count(file => file.Split(Path.DirectorySeparatorChar).Contains("src"));

        useCase.Cases["FileNameMatches"] = Sut.Interpolate(new Dictionary<string, string> { { "fileName", "codefile1" } }, true)
            .Count(file => file.Contains("codefile1"));

        useCase.Cases["ExtensionMatches"] = Sut.Interpolate(new Dictionary<string, string> { { "extension", "cs" } }, true)
            .Count(file => Path.GetExtension(file) == ".cs");

        var textPath = Sut
            .Interpolate(new Dictionary<string, string> { { "fileName", "updated" }, { "fileText", "Hello world" } }, hasTags: true, isFile: true)
            .Single(file => file.Contains("updated"));
        useCase.Cases["FileText"] = File.ReadAllText(textPath);
    }

    [Then("the interpolated folder count should be 1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesFileFolder() => useCase.Cases["FolderMatches"].Should().Be(1);

    [Then("the interpolated file name count should be 1")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_InterpolatesFileName() => useCase.Cases["FileNameMatches"].Should().Be(1);

    [Then("the interpolated extension count should be 1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_InterpolatesFileExtension() => useCase.Cases["ExtensionMatches"].Should().Be(1);

    [Then("the interpolated file text should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_InterpolatesFileText() => ((string)useCase.Cases["FileText"]!).Should().NotBeNullOrEmpty().And.Be("Hello world");
}
