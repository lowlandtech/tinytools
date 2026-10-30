namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC01_DiscoversTemplates : TinyToolsScenario<TemplateRegistry>
{
    private ITemplate? _component;
    private ITemplate? _class;
    private List<string> _names = [];

    protected override TemplateRegistry For() => new();

    [Given("an empty registry and the test assembly that holds the component and class templates")]
    protected override void Given()
    {
    }

    [When("the registry discovers the templates of the test assembly")]
    protected override void When()
    {
        base.When();

        Sut.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);

        _component = Sut.Get("Component");
        _class = Sut.Get("CSharpClass");
        _names = Sut.GetNames().ToList();
    }

    public SC01_DiscoversTemplates() => ArrangeAndAct();

    [Then("the discovered component template should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DiscoversComponentTemplate() => _component.Should().NotBeNull();

    [Then("the discovered class template should not be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DiscoversClassTemplate() => _class.Should().NotBeNull();

    [Then("the discovered names should contain Component and not ComponentTemplate")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RemovesTemplateSuffixFromName() => _names.Should().Contain("Component").And.NotContain("ComponentTemplate");

    [Then("the discovery should contain at least two templates")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DiscoversMultipleTemplates() => _names.Should().HaveCountGreaterThanOrEqualTo(2);

    [Then("the discovered component template should be ComponentTemplate")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ReturnsCorrectTemplateType() => _component.Should().BeOfType<ComponentTemplate>();
}
