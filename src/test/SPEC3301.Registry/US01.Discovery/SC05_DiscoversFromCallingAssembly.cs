namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "05")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC05_DiscoversFromCallingAssembly : TinyToolsScenario<TemplateRegistry>
{
    private ITemplate? _component;
    private List<string> _names = [];

    protected override TemplateRegistry For() => new();

    [Given("an empty registry that holds no template")]
    protected override void Given()
    {
    }

    [When("the registry discovers the templates of its calling assembly")]
    protected override void When()
    {
        base.When();

        Sut.DiscoverFromCallingAssembly();

        _component = Sut.Get("Component");
        _names = Sut.GetNames().ToList();
    }

    public SC05_DiscoversFromCallingAssembly() => ArrangeAndAct();

    [Then("the discovery should contain at least two templates")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DiscoversTemplatesFromTestAssembly() => _names.Should().HaveCountGreaterThanOrEqualTo(2);

    [Then("the discovered component template should not be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DiscoversComponentTemplate() => _component.Should().NotBeNull();
}
