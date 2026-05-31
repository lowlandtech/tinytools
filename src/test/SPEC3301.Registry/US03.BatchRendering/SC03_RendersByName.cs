namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

/// <summary>
/// Tests TemplateRegistry rendering by name.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "03")]
[UserStory("03", "Template registry renders templates by name")]
public class WhenRenderingTemplateByName : TinyToolsScenario<TemplateRegistry>
{
    private TemplateResult? _result;
    private ComponentData? _data;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        return registry;
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _data = new ComponentData
        {
            ComponentName = "Card",
            Props = new()
            {
                new PropDefinition { Name = "title", Type = "string" }
            },
            PropsDestructured = "{ title }"
        };
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render("component", _data!);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render Template")]
    [Fact]
    public void ItShouldRenderTemplate()
    {
        ArrangeAndAct();
        _result.Should().NotBeNull();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Contain Component Name")]
    [Fact]
    public void ItShouldContainComponentName()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("Card");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have Correct Path")]
    [Fact]
    public void ItShouldHaveCorrectPath()
    {
        ArrangeAndAct();
        _result!.Path.Should().Be("src/components/Card.tsx");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Return Null For Non Existent Template")]
    [Fact]
    public void ItShouldReturnNullForNonExistentTemplate()
    {
        ArrangeAndAct();
        var nullResult = Sut.Render("nonexistent", _data!);
        nullResult.Should().BeNull();
    }
}
