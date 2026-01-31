using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests template with empty props list.
/// </summary>
public class WhenRenderingComponentTemplateWithNoProps : WhenTestingFor<ComponentTemplate>
{
    private TemplateResult? _result;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
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
