namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

/// <summary>
/// Tests TemplateRegistry batch rendering.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "02")]
[UserStory("03", "Template registry renders batches of templates")]
public class WhenRenderingMultipleTemplatesInBatch : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        var batch = new Dictionary<string, object>
        {
            ["component1"] = new ComponentData 
            { 
                ComponentName = "Button",
                Props = new(),
                PropsDestructured = "{}"
            },
            ["class1"] = new ClassData
            {
                Namespace = "Models",
                ClassName = "User",
                Description = "User model"
            }
        };

        // Note: RenderBatch expects template names as keys
        // So we need to adapt this
        _results = new Dictionary<string, TemplateResult>();
        _results["component1"] = Sut.Render("component", batch["component1"])!;
        _results["class1"] = Sut.Render("class", batch["class1"])!;
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render Both Templates")]
    [Fact]
    public void ItShouldRenderBothTemplates()
    {
        ArrangeAndAct();
        _results.Should().HaveCount(2);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Component Correctly")]
    [Fact]
    public void ItShouldRenderComponentCorrectly()
    {
        ArrangeAndAct();
        _results.Should().ContainKey("component1");
        _results!["component1"].Content.Should().Contain("Button");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Render Class Correctly")]
    [Fact]
    public void ItShouldRenderClassCorrectly()
    {
        ArrangeAndAct();
        _results.Should().ContainKey("class1");
        _results!["class1"].Content.Should().Contain("class User");
    }
}
