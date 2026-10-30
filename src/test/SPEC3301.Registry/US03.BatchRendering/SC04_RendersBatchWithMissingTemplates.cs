namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "04")]
[UserStory("03", "Template registry renders batches of templates")]
public sealed class SC04_RendersBatchWithMissingTemplates : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.BatchRenderingUseCase>
{
    private readonly UseCases.BatchRenderingUseCase useCase;
    private Dictionary<string, object> _batch = [];

    public SC04_RendersBatchWithMissingTemplates(UseCases.BatchRenderingUseCase useCase)
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

    [Given("a registry without a class template and a batch that also asks for a missing template")]
    protected override void Given() => _batch = new Dictionary<string, object>
    {
        ["component"] = new ComponentData
        {
            ComponentName = "Card",
            Props = new(),
            PropsDestructured = "{}",
        },
        ["missing"] = new { Name = "Test" },
    };

    [When("the batch that asks for a missing template is rendered")]
    protected override void When()
    {
        base.When();

        useCase.Cases["Results"] = Sut.RenderBatch(_batch);
    }

    [Then("the batch results should have count 1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_OnlyReturnsRegisteredTemplates() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!).Should().HaveCount(1);

    [Then("the batch results should contain the component entry")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsExistingTemplate() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!).Should().ContainKey("component");

    [Then("the batch results should not contain the missing entry")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_OmitsMissingTemplate() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!).Should().NotContainKey("missing");

    [Then("the component content should contain Card")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersExistingTemplate() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!)["component"].Content.Should().Contain("Card");
}
