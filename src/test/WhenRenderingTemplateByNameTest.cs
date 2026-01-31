using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests TemplateRegistry rendering by name.
/// </summary>
public class WhenRenderingTemplateByName : WhenTestingFor<TemplateRegistry>
{
    private TemplateResult? _result;
    private ComponentData? _data;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        return registry;
    }

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

    protected override void When()
    {
        _result = Sut.Render("component", _data!);
    }

    [Fact]
    public void ItShouldRenderTemplate()
    {
        _result.Should().NotBeNull();
    }

    [Fact]
    public void ItShouldContainComponentName()
    {
        _result!.Content.Should().Contain("Card");
    }

    [Fact]
    public void ItShouldHaveCorrectPath()
    {
        _result!.Path.Should().Be("src/components/Card.tsx");
    }

    [Fact]
    public void ItShouldReturnNullForNonExistentTemplate()
    {
        var nullResult = Sut.Render("nonexistent", _data!);
        nullResult.Should().BeNull();
    }
}
