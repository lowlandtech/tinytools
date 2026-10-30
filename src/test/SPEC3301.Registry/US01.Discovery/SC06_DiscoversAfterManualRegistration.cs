namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "06")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC06_DiscoversAfterManualRegistration : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.DiscoveryUseCase>
{
    private readonly UseCases.DiscoveryUseCase useCase;
    private ComponentTemplate _manualTemplate = null!;

    public SC06_DiscoversAfterManualRegistration(UseCases.DiscoveryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["ManualTemplate"] = _manualTemplate;
        useCase.Cases["Discovered"] = Sut.Get("Component");
        useCase.Cases["ComponentNames"] = Sut.GetNames().Where(name => name == "Component").ToList();
    }

    [Then("the manual component registration should be replaced")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_OverwritesManualRegistration() => useCase.Cases["Discovered"].Should().NotBeNull().And.NotBeSameAs(useCase.Cases["ManualTemplate"]);

    [Then("the component registration should have count 1")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_KeepsOnlyOneComponentRegistration() => ((List<string>)useCase.Cases["ComponentNames"]!).Should().HaveCount(1);
}
