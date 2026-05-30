using LowlandTech.TinyTools.Tests.Shared.Examples;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

/// <summary>
/// Tests template with empty props list.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "02")]
[UserStory("02", "Component template generates React components")]
public class WhenRenderingComponentTemplateWithNoProps : WhenTestingFor<Shared.Examples.ComponentTemplate>
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
            ComponentName = "EmptyComponent",
            Props = new(),
            PropsDestructured = "{}"
        };
        _result = Sut.Render(data);
    }

    [Fact]
    public void ItShouldGenerateEmptyPropsInterface()
    {
        _result!.Content.Should().Contain("export interface EmptyComponentProps");
    }

    [Fact]
    public void ItShouldNotHavePropsInInterface()
    {
        // Interface should be empty (only has braces)
        _result!.Content.Should().MatchRegex(@"interface EmptyComponentProps \{\s*\}");
    }

    [Fact]
    public void ItShouldHaveEmptyDestructuring()
    {
        _result!.Content.Should().Contain("({})");
    }
}
