namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template registry renders batches of templates")]
public sealed class SC01_RendersBatch : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, object> _batch = [];
    private Dictionary<string, TemplateResult> _results = [];

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    [Given("a component model and a class model to render in one batch")]
    protected override void Given() => _batch = new Dictionary<string, object>
    {
        ["component"] = new ComponentData
        {
            ComponentName = "Button",
            Props = new() { new PropDefinition { Name = "label", Type = "string" } },
            PropsDestructured = "{ label }",
        },
        ["class"] = new ClassData
        {
            Namespace = "Models",
            ClassName = "User",
            Description = "User model",
        },
    };

    [When("the component model and the class model are rendered in one batch")]
    protected override void When()
    {
        base.When();

        _results = Sut.RenderBatch(_batch);
    }

    public SC01_RendersBatch() => ArrangeAndAct();

    [Then("the batch results should have count 2")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReturnsResultsForAllTemplates() => _results.Should().HaveCount(2);

    [Then("the batch results should contain the component entry")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsComponentResult() => _results.Should().ContainKey("component");

    [Then("the batch results should contain the class entry")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ContainsClassResult() => _results.Should().ContainKey("class");

    [Then("the component content should contain Button")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersComponentContent() => _results["component"].Content.Should().Contain("Button");

    [Then("the class content should contain User")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersClassContent() => _results["class"].Content.Should().Contain("User");

    [Then("the component path should contain Button")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_GeneratesComponentPath() => _results["component"].Path.Should().Contain("Button");

    [Then("the class path should contain User")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_GeneratesClassPath() => _results["class"].Path.Should().Contain("User");
}
