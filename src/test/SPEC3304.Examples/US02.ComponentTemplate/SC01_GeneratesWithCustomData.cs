namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

/// <summary>
/// Tests template rendering with custom data.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Component template generates React components")]
public class WhenRenderingComponentTemplateWithCustomData : TinyToolsScenario<Shared.Examples.ComponentTemplate>
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
            ComponentName = "Button",
            Props = new()
            {
                new PropDefinition { Name = "label", Type = "string" },
                new PropDefinition { Name = "onClick", Type = "() => void" },
                new PropDefinition { Name = "disabled", Type = "boolean" }
            },
            PropsDestructured = "{ label, onClick, disabled }"
        };
        _result = Sut.Render(data);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Generate Button Path")]
    [Fact]
    public void ItShouldGenerateButtonPath()
    {
        ArrangeAndAct();
        _result!.Path.Should().Be("src/components/Button.tsx");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Contain Button Interface")]
    [Fact]
    public void ItShouldContainButtonInterface()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("export interface ButtonProps");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Contain All Three Props")]
    [Fact]
    public void ItShouldContainAllThreeProps()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("label: string");
        _result!.Content.Should().Contain("onClick: () => void");
        _result!.Content.Should().Contain("disabled: boolean");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Contain Button Component")]
    [Fact]
    public void ItShouldContainButtonComponent()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("export const Button");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Use Lowercase Class Name Button")]
    [Fact]
    public void ItShouldUseLowercaseClassNameButton()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("className=\"button\"");
    }
}
