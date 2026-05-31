namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

/// <summary>
/// Tests template with empty props list.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "02")]
[UserStory("02", "Component template generates React components")]
public class WhenRenderingComponentTemplateWithNoProps : TinyToolsScenario<Shared.Examples.ComponentTemplate>
{
    private TemplateResult? _result;

    protected override Shared.Examples.ComponentTemplate For()
    {
        return new Shared.Examples.ComponentTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        var data = new ComponentData
        {
            ComponentName = "EmptyComponent",
            Props = new(),
            PropsDestructured = "{}"
        };
        _result = Sut.Render(data);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Generate Empty Props Interface")]
    [Fact]
    public void ItShouldGenerateEmptyPropsInterface()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("export interface EmptyComponentProps");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Not Have Props In Interface")]
    [Fact]
    public void ItShouldNotHavePropsInInterface()
    {
        ArrangeAndAct();
        // Interface should be empty (only has braces)
        _result!.Content.Should().MatchRegex(@"interface EmptyComponentProps \{\s*\}");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have Empty Destructuring")]
    [Fact]
    public void ItShouldHaveEmptyDestructuring()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("({})");
    }
}
