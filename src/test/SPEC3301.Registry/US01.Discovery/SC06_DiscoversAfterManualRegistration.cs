namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "06")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC06_DiscoversAfterManualRegistration : TinyToolsScenario<TemplateRegistry>
{
    private ComponentTemplate _manualTemplate = null!;
    private ITemplate? _discovered;
    private List<string> _componentNames = [];

    protected override TemplateRegistry For()
    {
        _manualTemplate = new ComponentTemplate();

        var registry = new TemplateRegistry();
        registry.Register("Component", _manualTemplate);
        return registry;
    }

    [Given("a registry holding a manually registered component template")]
    protected override void Given()
    {
    }

    [When("the registry discovers the templates of the test assembly")]
    protected override void When()
    {
        base.When();

        Sut.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);

        _discovered = Sut.Get("Component");
        _componentNames = Sut.GetNames().Where(name => name == "Component").ToList();
    }

    public SC06_DiscoversAfterManualRegistration() => ArrangeAndAct();

    [Then("the manual component registration should be replaced")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_OverwritesManualRegistration() => _discovered.Should().NotBeNull().And.NotBeSameAs(_manualTemplate);

    [Then("the component registration should have count 1")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_KeepsOnlyOneComponentRegistration() => _componentNames.Should().HaveCount(1);
}
