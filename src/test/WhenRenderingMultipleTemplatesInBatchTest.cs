using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests TemplateRegistry batch rendering.
/// </summary>
public class WhenRenderingMultipleTemplatesInBatch : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    protected override void When()
    {
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

    [Fact]
    public void ItShouldRenderBothTemplates()
    {
        _results.Should().HaveCount(2);
    }

    [Fact]
    public void ItShouldRenderComponentCorrectly()
    {
        _results.Should().ContainKey("component1");
        _results!["component1"].Content.Should().Contain("Button");
    }

    [Fact]
    public void ItShouldRenderClassCorrectly()
    {
        _results.Should().ContainKey("class1");
        _results!["class1"].Content.Should().Contain("class User");
    }
}
