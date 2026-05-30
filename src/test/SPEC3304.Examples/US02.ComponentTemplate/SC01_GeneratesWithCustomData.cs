using LowlandTech.TinyTools.Tests.Shared.Examples;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

/// <summary>
/// Tests template rendering with custom data.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Component template generates React components")]
public class WhenRenderingComponentTemplateWithCustomData : WhenTestingFor<Shared.Examples.ComponentTemplate>
{
    private TemplateResult? _result;

    protected override Shared.Examples.ComponentTemplate For()
    {
        return new Shared.Examples.ComponentTemplate();
    }

    protected override void When()
    {
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

    [Fact]
    public void ItShouldGenerateButtonPath()
    {
        _result!.Path.Should().Be("src/components/Button.tsx");
    }

    [Fact]
    public void ItShouldContainButtonInterface()
    {
        _result!.Content.Should().Contain("export interface ButtonProps");
    }

    [Fact]
    public void ItShouldContainAllThreeProps()
    {
        _result!.Content.Should().Contain("label: string");
        _result!.Content.Should().Contain("onClick: () => void");
        _result!.Content.Should().Contain("disabled: boolean");
    }

    [Fact]
    public void ItShouldContainButtonComponent()
    {
        _result!.Content.Should().Contain("export const Button");
    }

    [Fact]
    public void ItShouldUseLowercaseClassNameButton()
    {
        _result!.Content.Should().Contain("className=\"button\"");
    }
}
