namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "03")]
[UserStory("03", "Template registry renders templates by name")]
public sealed class SC03_RendersByName : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.BatchRenderingUseCase>
{
    private readonly UseCases.BatchRenderingUseCase useCase;
    private ComponentData _data = null!;

    public SC03_RendersByName(UseCases.BatchRenderingUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        return registry;
    }

    [Given("component data describing a Card with a title property")]
    protected override void Given() => _data = new ComponentData
    {
        ComponentName = "Card",
        Props = new() { new PropDefinition { Name = "title", Type = "string" } },
        PropsDestructured = "{ title }",
    };

    [When("the component template is rendered by name and an unknown name is rendered")]
    protected override void When()
    {
        base.When();

        useCase.Cases["Result"] = Sut.Render("component", _data);
        useCase.Cases["Missing"] = Sut.Render("nonexistent", _data);
    }

    [Then("the rendered result should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTemplate() => useCase.Cases["Result"].Should().NotBeNull();

    [Then("the rendered content should contain Card")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsComponentName() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("Card");

    [Then("the rendered path should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasCorrectPath() => ((TemplateResult)useCase.Cases["Result"]!).Path.Should().Be("src/components/Card.tsx");

    [Then("the missing template should be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ReturnsNullForNonExistentTemplate() => useCase.Cases["Missing"].Should().BeNull();
}
