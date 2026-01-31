using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests for TemplateRegistry.RenderBatch method.
/// </summary>
public class WhenRenderingBatchOfTemplates : WhenTestingFor<TemplateRegistry>
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
            ["component"] = new ComponentData 
            { 
                ComponentName = "Button",
                Props = new List<PropDefinition>
                {
                    new PropDefinition { Name = "label", Type = "string" }
                },
                PropsDestructured = "{ label }"
            },
            ["class"] = new ClassData
            {
                Namespace = "Models",
                ClassName = "User",
                Description = "User model"
            }
        };

        _results = Sut.RenderBatch(batch);
    }

    [Fact]
    public void ItShouldReturnResultsForAllTemplates()
    {
        _results.Should().HaveCount(2);
    }

    [Fact]
    public void ItShouldContainComponentResult()
    {
        _results.Should().ContainKey("component");
    }

    [Fact]
    public void ItShouldContainClassResult()
    {
        _results.Should().ContainKey("class");
    }

    [Fact]
    public void ItShouldRenderComponentContent()
    {
        _results!["component"].Content.Should().Contain("Button");
    }

    [Fact]
    public void ItShouldRenderClassContent()
    {
        _results!["class"].Content.Should().Contain("User");
    }

    [Fact]
    public void ItShouldGenerateComponentPath()
    {
        _results!["component"].Path.Should().Contain("Button");
    }

    [Fact]
    public void ItShouldGenerateClassPath()
    {
        _results!["class"].Path.Should().Contain("User");
    }
}

/// <summary>
/// Tests RenderBatch with missing templates.
/// </summary>
public class WhenRenderingBatchWithMissingTemplates : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        // Don't register "class" template
        return registry;
    }

    protected override void When()
    {
        var batch = new Dictionary<string, object>
        {
            ["component"] = new ComponentData 
            { 
                ComponentName = "Card",
                Props = new List<PropDefinition>(),
                PropsDestructured = "{}"
            },
            ["missing"] = new { Name = "Test" } // This template doesn't exist
        };

        _results = Sut.RenderBatch(batch);
    }

    [Fact]
    public void ItShouldOnlyReturnResultsForRegisteredTemplates()
    {
        _results.Should().HaveCount(1);
    }

    [Fact]
    public void ItShouldContainResultForExistingTemplate()
    {
        _results.Should().ContainKey("component");
    }

    [Fact]
    public void ItShouldNotContainResultForMissingTemplate()
    {
        _results.Should().NotContainKey("missing");
    }

    [Fact]
    public void ItShouldRenderExistingTemplateCorrectly()
    {
        _results!["component"].Content.Should().Contain("Card");
    }
}

/// <summary>
/// Tests RenderBatch with empty batch.
/// </summary>
public class WhenRenderingEmptyBatch : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        return registry;
    }

    protected override void When()
    {
        var batch = new Dictionary<string, object>();
        _results = Sut.RenderBatch(batch);
    }

    [Fact]
    public void ItShouldReturnEmptyResults()
    {
        _results.Should().BeEmpty();
    }

    [Fact]
    public void ItShouldNotBeNull()
    {
        _results.Should().NotBeNull();
    }
}

/// <summary>
/// Tests RenderBatch with single template.
/// </summary>
public class WhenRenderingSingleTemplateBatch : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    protected override void When()
    {
        var batch = new Dictionary<string, object>
        {
            ["class"] = new ClassData
            {
                Namespace = "MyApp.Services",
                ClassName = "OrderService",
                Description = "Handles order operations",
                Properties = new List<PropertyData>
                {
                    new PropertyData { Name = "Id", Type = "int", Description = "Order ID" }
                }
            }
        };

        _results = Sut.RenderBatch(batch);
    }

    [Fact]
    public void ItShouldReturnSingleResult()
    {
        _results.Should().HaveCount(1);
    }

    [Fact]
    public void ItShouldRenderClassWithProperties()
    {
        _results!["class"].Content.Should().Contain("OrderService");
        _results!["class"].Content.Should().Contain("int Id");
    }

    [Fact]
    public void ItShouldHaveCorrectNamespace()
    {
        _results!["class"].Namespace.Should().Be("MyApp.Services");
    }
}

/// <summary>
/// Tests RenderBatch with multiple instances of same template.
/// </summary>
public class WhenRenderingMultipleInstancesOfSameTemplate : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        return registry;
    }

    protected override void When()
    {
        // RenderBatch uses template names as keys, so we can only have one result per template name
        var batch = new Dictionary<string, object>
        {
            ["component"] = new ComponentData 
            { 
                ComponentName = "LastButton",
                Props = new List<PropDefinition>(),
                PropsDestructured = "{}"
            }
        };

        _results = Sut.RenderBatch(batch);
    }

    [Fact]
    public void ItShouldRenderTheTemplate()
    {
        _results.Should().HaveCount(1);
    }

    [Fact]
    public void ItShouldContainCorrectContent()
    {
        _results!["component"].Content.Should().Contain("LastButton");
    }
}
