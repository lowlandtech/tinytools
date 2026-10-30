namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "02")]
[UserStory("03", "Template registry renders batches of templates")]
public sealed class SC02_RendersMultipleTemplatesBatch : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateResult> _results = [];

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    [Given("a registry holding the component and class templates")]
    protected override void Given()
    {
    }

    [When("a component and a class model are rendered in one batch")]
    protected override void When()
    {
        base.When();

        var batch = new Dictionary<string, object>
        {
            ["component1"] = new ComponentData
            {
                ComponentName = "Button",
                Props = new(),
                PropsDestructured = "{}",
            },
            ["class1"] = new ClassData
            {
                Namespace = "Models",
                ClassName = "User",
                Description = "User model",
            },
        };

        _results["component1"] = Sut.Render("component", batch["component1"])!;
        _results["class1"] = Sut.Render("class", batch["class1"])!;
    }

    public SC02_RendersMultipleTemplatesBatch() => ArrangeAndAct();

    [Then("the batch results should have count 2")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersBothTemplates() => _results.Should().HaveCount(2);

    [Then("the component batch entry should contain Button")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersComponentCorrectly() => _results["component1"].Content.Should().Contain("Button");

    [Then("the class batch entry should contain class User")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersClassCorrectly() => _results["class1"].Content.Should().Contain("class User");
}
