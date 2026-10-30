namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "04")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC04_RendersDiscoveredTemplates : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.DiscoveryUseCase>
{
    private readonly UseCases.DiscoveryUseCase useCase;
    private ComponentData _data = null!;

    public SC04_RendersDiscoveredTemplates(UseCases.DiscoveryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
        return registry;
    }

    [Given("component data describing a DiscoveredButton with an onClick property")]
    protected override void Given() => _data = new ComponentData
    {
        ComponentName = "DiscoveredButton",
        Props = new() { new PropDefinition { Name = "onClick", Type = "() => void" } },
        PropsDestructured = "{ onClick }",
    };

    [When("the discovered component template is rendered by name")]
    protected override void When()
    {
        base.When();

        useCase.Cases["Result"] = Sut.Render("Component", _data);
    }

    [Then("the rendered result should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheDiscoveredTemplate() => useCase.Cases["Result"].Should().NotBeNull();

    [Then("the rendered content should contain DiscoveredButton")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsExpectedContent() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("DiscoveredButton");

    [Then("the rendered path should contain DiscoveredButton")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_GeneratesPath() => ((TemplateResult)useCase.Cases["Result"]!).Path.Should().Contain("DiscoveredButton");
}
