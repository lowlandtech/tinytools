namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US02.Registration;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template registry supports manual registration")]
public sealed class SC01_RegistersAndRetrieves : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.RegistrationUseCase>
{
    private readonly UseCases.RegistrationUseCase useCase;
    private ComponentTemplate _componentTemplate = null!;
    private CSharpClassTemplate _classTemplate = null!;

    public SC01_RegistersAndRetrieves(UseCases.RegistrationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["ComponentTemplate"] = _componentTemplate;
        useCase.Cases["ClassTemplate"] = _classTemplate;
        useCase.Cases["Component"] = Sut.Get("component");
        useCase.Cases["Class"] = Sut.Get("class");
        useCase.Cases["Missing"] = Sut.Get("nonexistent");
        useCase.Cases["Names"] = Sut.GetNames().ToList();
        useCase.Cases["All"] = Sut.GetAll().ToList();
    }

    [Then("the component template should be registered")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RetrievesComponentTemplate() => useCase.Cases["Component"].Should().BeSameAs(useCase.Cases["ComponentTemplate"]);

    [Then("the class template should be registered")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RetrievesCSharpClassTemplate() => useCase.Cases["Class"].Should().BeSameAs(useCase.Cases["ClassTemplate"]);

    [Then("the unregistered template should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReturnsNullForUnregisteredTemplate() => useCase.Cases["Missing"].Should().BeNull();

    [Then("the registered names should have count 2")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ListsAllRegisteredNames() => ((List<string>)useCase.Cases["Names"]!).Should().BeEquivalentTo("component", "class");

    [Then("the registered templates should have count 2")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_GetsAllTemplates() => ((List<ITemplate>)useCase.Cases["All"]!).Should().BeEquivalentTo(new ITemplate[] { (ITemplate)useCase.Cases["ComponentTemplate"]!, (ITemplate)useCase.Cases["ClassTemplate"]! });
}
