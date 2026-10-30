namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "14")]
[UserStory("01", "String interpolation processes files without tags")]
public sealed class SC14_InterpolatesFilesUntagged : TinyToolsScenario<List<string>>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private string _root = null!;
    private string _folder = null!;

    public SC14_InterpolatesFilesUntagged(UseCases.StringInterpolationUseCase useCase)
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

    [When("the file names and texts are interpolated without tags once per case")]
    protected override void When()
    {
        base.When();

        useCase.Cases["SubfolderMatches"] = Sut.Interpolate(new Dictionary<string, string> { { "subfolder1", "src" }, { "subfolder2", "lowlandtech" } })
            .Count(file => file.Contains(Path.Combine("src", "lowlandtech")));

        var names = new Dictionary<string, string> { { "file1", "codefile1" }, { "file2", "codefile2" } };
        useCase.Cases["NamePair"] = (
            Sut.Interpolate(names).Count(file => file.Contains("codefile1")),
            Sut.Interpolate(names).Count(file => file.Contains("codefile2")));

        useCase.Cases["ExtensionMatches"] = Sut.Interpolate(new Dictionary<string, string> { { ".txt", ".cs" } })
            .Count(file => file.Contains(".cs"));

        var textPath = Sut
            .Interpolate(new Dictionary<string, string> { { "file3", "updated" }, { "fileText", "Hello world" } }, hasTags: false, isFile: true)
            .Single(file => file.Contains("updated"));
        useCase.Cases["FileText"] = File.ReadAllText(textPath);
    }

    [Then("the interpolated subfolder count should be 2")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesFileFolder() => useCase.Cases["SubfolderMatches"].Should().Be(2);

    [Then("the interpolated name pair should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_InterpolatesFileName() => ((ValueTuple<int, int>)useCase.Cases["NamePair"]!).Should().Be((3, 3));

    [Then("the interpolated extension count should be 7")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_InterpolatesFileExtension() => useCase.Cases["ExtensionMatches"].Should().Be(7);

    [Then("the interpolated file text should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_InterpolatesFileText() => ((string)useCase.Cases["FileText"]!).Should().NotBeNullOrEmpty().And.Be("Hello world");

    public override void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        base.Dispose();
    }
}
