namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US02.Registration;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template registry supports manual registration")]
public sealed class SC01_RegistersAndRetrieves : TinyToolsScenario<TemplateRegistry>
{
    private ComponentTemplate _componentTemplate = null!;
    private CSharpClassTemplate _classTemplate = null!;
    private ITemplate? _component;
    private ITemplate? _class;
    private ITemplate? _missing;
    private List<string> _names = [];
    private List<ITemplate> _all = [];

    protected override TemplateRegistry For() => new();

    [Given("a component template and a C# class template")]
    protected override void Given()
    {
        _componentTemplate = new ComponentTemplate();
        _classTemplate = new CSharpClassTemplate();
    }

    [When("both templates are registered and read back")]
    protected override void When()
    {
        base.When();

        Sut.Register("component", _componentTemplate);
        Sut.Register("class", _classTemplate);

        _component = Sut.Get("component");
        _class = Sut.Get("class");
        _missing = Sut.Get("nonexistent");
        _names = Sut.GetNames().ToList();
        _all = Sut.GetAll().ToList();
    }

    public SC01_RegistersAndRetrieves() => ArrangeAndAct();

    [Then("the component template should be registered")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RetrievesComponentTemplate() => _component.Should().BeSameAs(_componentTemplate);

    [Then("the class template should be registered")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RetrievesCSharpClassTemplate() => _class.Should().BeSameAs(_classTemplate);

    [Then("the unregistered template should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReturnsNullForUnregisteredTemplate() => _missing.Should().BeNull();

    [Then("the registered names should have count 2")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ListsAllRegisteredNames() => _names.Should().BeEquivalentTo("component", "class");

    [Then("the registered templates should have count 2")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_GetsAllTemplates() => _all.Should().BeEquivalentTo(new ITemplate[] { _componentTemplate, _classTemplate });
}
