namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

/// <summary>
/// Tests for TemplateRegistry.DiscoverFromAssembly method.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public class WhenDiscoveringTemplatesFromAssembly : TinyToolsScenario<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        // Discover templates from the test assembly which contains ComponentTemplate and CSharpClassTemplate
        Sut.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Discover Component Template")]
    [Fact]
    public void ItShouldDiscoverComponentTemplate()
    {
        ArrangeAndAct();
        Sut.Get("Component").Should().NotBeNull();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Discover C Sharp Class Template")]
    [Fact]
    public void ItShouldDiscoverCSharpClassTemplate()
    {
        ArrangeAndAct();
        Sut.Get("CSharpClass").Should().NotBeNull();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Remove Template Suffix From Name")]
    [Fact]
    public void ItShouldRemoveTemplateSuffixFromName()
    {
        ArrangeAndAct();
        // ComponentTemplate becomes "Component"
        Sut.GetNames().Should().Contain("Component");
        Sut.GetNames().Should().NotContain("ComponentTemplate");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Discover Multiple Templates")]
    [Fact]
    public void ItShouldDiscoverMultipleTemplates()
    {
        ArrangeAndAct();
        Sut.GetNames().Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Return Correct Template Type")]
    [Fact]
    public void ItShouldReturnCorrectTemplateType()
    {
        ArrangeAndAct();
        Sut.Get("Component").Should().BeOfType<ComponentTemplate>();
    }
}

/// <summary>
/// Tests that DiscoverFromAssembly skips abstract and interface types.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public class WhenDiscoveringTemplatesSkipsAbstractTypes : TinyToolsScenario<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        Sut.DiscoverFromAssembly(typeof(TemplateBase).Assembly);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Not Discover Template Base As Template")]
    [Fact]
    public void ItShouldNotDiscoverTemplateBaseAsTemplate()
    {
        ArrangeAndAct();
        // TemplateBase is abstract and should not be discovered
        Sut.Get("Base").Should().BeNull();
        Sut.Get("TemplateBase").Should().BeNull();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Not Discover I Template Interface")]
    [Fact]
    public void ItShouldNotDiscoverITemplateInterface()
    {
        ArrangeAndAct();
        Sut.Get("ITemplate").Should().BeNull();
        Sut.Get("I").Should().BeNull();
    }
}

/// <summary>
/// Tests DiscoverFromAssembly with empty or no templates.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public class WhenDiscoveringFromAssemblyWithNoTemplates : TinyToolsScenario<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        // System assembly has no ITemplate implementations
        Sut.DiscoverFromAssembly(typeof(string).Assembly);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Have No Templates")]
    [Fact]
    public void ItShouldHaveNoTemplates()
    {
        ArrangeAndAct();
        Sut.GetNames().Should().BeEmpty();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Not Throw")]
    [Fact]
    public void ItShouldNotThrow()
    {
        ArrangeAndAct();
        // The When() method already ran without throwing
        Sut.GetAll().Should().BeEmpty();
    }
}

/// <summary>
/// Tests that discovered templates can be rendered.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public class WhenRenderingDiscoveredTemplates : TinyToolsScenario<TemplateRegistry>
{
    private TemplateResult? _result;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        var data = new ComponentData
        {
            ComponentName = "DiscoveredButton",
            Props = new List<PropDefinition>
            {
                new PropDefinition { Name = "onClick", Type = "() => void" }
            },
            PropsDestructured = "{ onClick }"
        };

        _result = Sut.Render("Component", data);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render The Discovered Template")]
    [Fact]
    public void ItShouldRenderTheDiscoveredTemplate()
    {
        ArrangeAndAct();
        _result.Should().NotBeNull();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Contain Expected Content")]
    [Fact]
    public void ItShouldContainExpectedContent()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("DiscoveredButton");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Generate Path")]
    [Fact]
    public void ItShouldGeneratePath()
    {
        ArrangeAndAct();
        _result!.Path.Should().Contain("DiscoveredButton");
    }
}

/// <summary>
/// Tests DiscoverFromCallingAssembly method.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public class WhenDiscoveringFromCallingAssembly : TinyToolsScenario<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        // This will discover from the test assembly
        Sut.DiscoverFromCallingAssembly();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Discover Templates From Test Assembly")]
    [Fact]
    public void ItShouldDiscoverTemplatesFromTestAssembly()
    {
        ArrangeAndAct();
        // The test assembly contains ComponentTemplate and CSharpClassTemplate
        Sut.GetNames().Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Discover Component Template")]
    [Fact]
    public void ItShouldDiscoverComponentTemplate()
    {
        ArrangeAndAct();
        Sut.Get("Component").Should().NotBeNull();
    }
}

/// <summary>
/// Tests that discovery doesn't duplicate existing registrations.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public class WhenDiscoveringAfterManualRegistration : TinyToolsScenario<TemplateRegistry>
{
    private ComponentTemplate _manualTemplate = null!;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        _manualTemplate = new ComponentTemplate();
        registry.Register("Component", _manualTemplate);
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        // Discover will overwrite existing registration with same name
        Sut.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Overwrite Manual Registration")]
    [Fact]
    public void ItShouldOverwriteManualRegistration()
    {
        ArrangeAndAct();
        // Discovery creates a new instance, so it won't be the same reference
        var discovered = Sut.Get("Component");
        discovered.Should().NotBeNull();
        // It should be a different instance (discovery creates new)
        discovered.Should().NotBeSameAs(_manualTemplate);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Still Have Only One Component Registration")]
    [Fact]
    public void ItShouldStillHaveOnlyOneComponentRegistration()
    {
        ArrangeAndAct();
        var names = Sut.GetNames().Where(n => n == "Component").ToList();
        names.Should().HaveCount(1);
    }
}

/// <summary>
/// Tests ValidateAll on discovered templates.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public class WhenValidatingDiscoveredTemplates : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _validationResults = Sut.ValidateAll();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Validate All Discovered Templates")]
    [Fact]
    public void ItShouldValidateAllDiscoveredTemplates()
    {
        ArrangeAndAct();
        _validationResults.Should().NotBeEmpty();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Have Valid Component Template")]
    [Fact]
    public void ItShouldHaveValidComponentTemplate()
    {
        ArrangeAndAct();
        _validationResults.Should().ContainKey("Component");
        _validationResults!["Component"].IsValid.Should().BeTrue();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have Valid C Sharp Class Template")]
    [Fact]
    public void ItShouldHaveValidCSharpClassTemplate()
    {
        ArrangeAndAct();
        _validationResults.Should().ContainKey("CSharpClass");
        _validationResults!["CSharpClass"].IsValid.Should().BeTrue();
    }
}

/// <summary>
/// Tests discovery with multiple calls to same assembly.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public class WhenDiscoveringFromSameAssemblyMultipleTimes : TinyToolsScenario<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        var assembly = typeof(ComponentTemplate).Assembly;
        Sut.DiscoverFromAssembly(assembly);
        Sut.DiscoverFromAssembly(assembly); // Discover again
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Not Duplicate Templates")]
    [Fact]
    public void ItShouldNotDuplicateTemplates()
    {
        ArrangeAndAct();
        var componentCount = Sut.GetNames().Count(n => n == "Component");
        componentCount.Should().Be(1);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Still Have All Templates")]
    [Fact]
    public void ItShouldStillHaveAllTemplates()
    {
        ArrangeAndAct();
        Sut.Get("Component").Should().NotBeNull();
        Sut.Get("CSharpClass").Should().NotBeNull();
    }
}
