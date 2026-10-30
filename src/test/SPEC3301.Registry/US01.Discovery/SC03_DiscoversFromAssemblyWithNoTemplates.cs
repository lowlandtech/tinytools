namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC03_DiscoversFromAssemblyWithNoTemplates : TinyToolsScenario<TemplateRegistry>
{
    private List<string> _names = [];
    private List<ITemplate> _all = [];

    protected override TemplateRegistry For() => new();

    [Given("an empty registry and an assembly that holds no template")]
    protected override void Given()
    {
    }

    [When("the registry discovers the templates of the system assembly")]
    protected override void When()
    {
        base.When();

        Sut.DiscoverFromAssembly(typeof(string).Assembly);

        _names = Sut.GetNames().ToList();
        _all = Sut.GetAll().ToList();
    }

    public SC03_DiscoversFromAssemblyWithNoTemplates() => ArrangeAndAct();

    [Then("the discovered names should have count 0")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DiscoversNoTemplates() => _names.Should().BeEmpty();

    [Then("the discovered templates should have count 0")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DoesNotThrow() => _all.Should().BeEmpty();
}
