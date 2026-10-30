namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "05")]
[UserStory("03", "Template registry renders batches of templates")]
public sealed class SC05_RendersEmptyBatch : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.BatchRenderingUseCase>
{
    private readonly UseCases.BatchRenderingUseCase useCase;
    private Dictionary<string, object> _batch = [];

    public SC05_RendersEmptyBatch(UseCases.BatchRenderingUseCase useCase)
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

    [Given("an empty batch of template data")]
    protected override void Given()
    {
    }

    [When("the empty batch is rendered")]
    protected override void When()
    {
        base.When();

        useCase.Cases["Results"] = Sut.RenderBatch(_batch);
    }

    [Then("the batch results should have count 0")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReturnsEmptyResults() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!).Should().BeEmpty();

    [Then("the batch results should not be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DoesNotReturnNull() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!).Should().NotBeNull();
}
