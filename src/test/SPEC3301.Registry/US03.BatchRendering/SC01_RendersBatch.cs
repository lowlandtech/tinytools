namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

/// <summary>
/// Tests for TemplateRegistry.RenderBatch method.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template registry renders batches of templates")]
public class WhenRenderingBatchOfTemplates : TinyToolsScenario<TemplateRegistry>
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

    [Trait(Spec.UAC, "01")]
    [Then("it Should Return Results For All Templates")]
    [Fact]
    public void ItShouldReturnResultsForAllTemplates()
    {
        ArrangeAndAct();
        _results.Should().HaveCount(2);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Contain Component Result")]
    [Fact]
    public void ItShouldContainComponentResult()
    {
        ArrangeAndAct();
        _results.Should().ContainKey("component");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Contain Class Result")]
    [Fact]
    public void ItShouldContainClassResult()
    {
        ArrangeAndAct();
        _results.Should().ContainKey("class");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Render Component Content")]
    [Fact]
    public void ItShouldRenderComponentContent()
    {
        ArrangeAndAct();
        _results!["component"].Content.Should().Contain("Button");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Render Class Content")]
    [Fact]
    public void ItShouldRenderClassContent()
    {
        ArrangeAndAct();
        _results!["class"].Content.Should().Contain("User");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Generate Component Path")]
    [Fact]
    public void ItShouldGenerateComponentPath()
    {
        ArrangeAndAct();
        _results!["component"].Path.Should().Contain("Button");
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Generate Class Path")]
    [Fact]
    public void ItShouldGenerateClassPath()
    {
        ArrangeAndAct();
        _results!["class"].Path.Should().Contain("User");
    }
}

/// <summary>
/// Tests RenderBatch with missing templates.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template registry renders batches of templates")]
public class WhenRenderingBatchWithMissingTemplates : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        // Don't register "class" template
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
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

    [Trait(Spec.UAC, "01")]
    [Then("it Should Only Return Results For Registered Templates")]
    [Fact]
    public void ItShouldOnlyReturnResultsForRegisteredTemplates()
    {
        ArrangeAndAct();
        _results.Should().HaveCount(1);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Contain Result For Existing Template")]
    [Fact]
    public void ItShouldContainResultForExistingTemplate()
    {
        ArrangeAndAct();
        _results.Should().ContainKey("component");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Not Contain Result For Missing Template")]
    [Fact]
    public void ItShouldNotContainResultForMissingTemplate()
    {
        ArrangeAndAct();
        _results.Should().NotContainKey("missing");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Render Existing Template Correctly")]
    [Fact]
    public void ItShouldRenderExistingTemplateCorrectly()
    {
        ArrangeAndAct();
        _results!["component"].Content.Should().Contain("Card");
    }
}

/// <summary>
/// Tests RenderBatch with empty batch.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template registry renders batches of templates")]
public class WhenRenderingEmptyBatch : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        var batch = new Dictionary<string, object>();
        _results = Sut.RenderBatch(batch);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Return Empty Results")]
    [Fact]
    public void ItShouldReturnEmptyResults()
    {
        ArrangeAndAct();
        _results.Should().BeEmpty();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Not Be Null")]
    [Fact]
    public void ItShouldNotBeNull()
    {
        ArrangeAndAct();
        _results.Should().NotBeNull();
    }
}

/// <summary>
/// Tests RenderBatch with single template.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template registry renders batches of templates")]
public class WhenRenderingSingleTemplateBatch : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
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

    [Trait(Spec.UAC, "01")]
    [Then("it Should Return Single Result")]
    [Fact]
    public void ItShouldReturnSingleResult()
    {
        ArrangeAndAct();
        _results.Should().HaveCount(1);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Class With Properties")]
    [Fact]
    public void ItShouldRenderClassWithProperties()
    {
        ArrangeAndAct();
        _results!["class"].Content.Should().Contain("OrderService");
        _results!["class"].Content.Should().Contain("int Id");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have Correct Namespace")]
    [Fact]
    public void ItShouldHaveCorrectNamespace()
    {
        ArrangeAndAct();
        _results!["class"].Namespace.Should().Be("MyApp.Services");
    }
}

/// <summary>
/// Tests RenderBatch with multiple instances of same template.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template registry renders batches of templates")]
public class WhenRenderingMultipleInstancesOfSameTemplate : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateResult>? _results;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
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

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render The Template")]
    [Fact]
    public void ItShouldRenderTheTemplate()
    {
        ArrangeAndAct();
        _results.Should().HaveCount(1);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Contain Correct Content")]
    [Fact]
    public void ItShouldContainCorrectContent()
    {
        ArrangeAndAct();
        _results!["component"].Content.Should().Contain("LastButton");
    }
}
