namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "07")]
[UserStory("03", "Template registry renders batches of templates")]
public sealed class SC07_RendersMultipleInstancesOfSameTemplate : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.BatchRenderingUseCase>
{
    private readonly UseCases.BatchRenderingUseCase useCase;
    private Dictionary<string, object> _batch = [];

    public SC07_RendersMultipleInstancesOfSameTemplate(UseCases.BatchRenderingUseCase useCase)
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

    [Given("component data describing a LastButton")]
    protected override void Given() => _batch = new Dictionary<string, object>
    {
        ["component"] = new ComponentData
        {
            ComponentName = "LastButton",
            Props = new(),
            PropsDestructured = "{}",
        },
    };

    [When("the component data is rendered in one batch")]
    protected override void When()
    {
        base.When();

        useCase.Cases["Results"] = Sut.RenderBatch(_batch);
    }

    [Then("the batch results should have count 1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheTemplate() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!).Should().HaveCount(1);

    [Then("the component content should contain LastButton")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsCorrectContent() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!)["component"].Content.Should().Contain("LastButton");
}
