using System.Reflection;
using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests for TemplateRegistry.DiscoverFromAssembly method.
/// </summary>
public class WhenDiscoveringTemplatesFromAssembly : WhenTestingFor<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    protected override void When()
    {
        // Discover templates from the test assembly which contains ComponentTemplate and CSharpClassTemplate
        Sut.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
    }

    [Fact]
    public void ItShouldDiscoverComponentTemplate()
    {
        Sut.Get("Component").Should().NotBeNull();
    }

    [Fact]
    public void ItShouldDiscoverCSharpClassTemplate()
    {
        Sut.Get("CSharpClass").Should().NotBeNull();
    }

    [Fact]
    public void ItShouldRemoveTemplateSuffixFromName()
    {
        // ComponentTemplate becomes "Component"
        Sut.GetNames().Should().Contain("Component");
        Sut.GetNames().Should().NotContain("ComponentTemplate");
    }

    [Fact]
    public void ItShouldDiscoverMultipleTemplates()
    {
        Sut.GetNames().Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void ItShouldReturnCorrectTemplateType()
    {
        Sut.Get("Component").Should().BeOfType<ComponentTemplate>();
    }
}

/// <summary>
/// Tests that DiscoverFromAssembly skips abstract and interface types.
/// </summary>
public class WhenDiscoveringTemplatesSkipsAbstractTypes : WhenTestingFor<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    protected override void When()
    {
        Sut.DiscoverFromAssembly(typeof(TemplateBase).Assembly);
    }

    [Fact]
    public void ItShouldNotDiscoverTemplateBaseAsTemplate()
    {
        // TemplateBase is abstract and should not be discovered
        Sut.Get("Base").Should().BeNull();
        Sut.Get("TemplateBase").Should().BeNull();
    }

    [Fact]
    public void ItShouldNotDiscoverITemplateInterface()
    {
        Sut.Get("ITemplate").Should().BeNull();
        Sut.Get("I").Should().BeNull();
    }
}

/// <summary>
/// Tests DiscoverFromAssembly with empty or no templates.
/// </summary>
public class WhenDiscoveringFromAssemblyWithNoTemplates : WhenTestingFor<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    protected override void When()
    {
        // System assembly has no ITemplate implementations
        Sut.DiscoverFromAssembly(typeof(string).Assembly);
    }

    [Fact]
    public void ItShouldHaveNoTemplates()
    {
        Sut.GetNames().Should().BeEmpty();
    }

    [Fact]
    public void ItShouldNotThrow()
    {
        // The When() method already ran without throwing
        Sut.GetAll().Should().BeEmpty();
    }
}

/// <summary>
/// Tests that discovered templates can be rendered.
/// </summary>
public class WhenRenderingDiscoveredTemplates : WhenTestingFor<TemplateRegistry>
{
    private TemplateResult? _result;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
        return registry;
    }

    protected override void When()
    {
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

    [Fact]
    public void ItShouldRenderTheDiscoveredTemplate()
    {
        _result.Should().NotBeNull();
    }

    [Fact]
    public void ItShouldContainExpectedContent()
    {
        _result!.Content.Should().Contain("DiscoveredButton");
    }

    [Fact]
    public void ItShouldGeneratePath()
    {
        _result!.Path.Should().Contain("DiscoveredButton");
    }
}

/// <summary>
/// Tests DiscoverFromCallingAssembly method.
/// </summary>
public class WhenDiscoveringFromCallingAssembly : WhenTestingFor<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    protected override void When()
    {
        // This will discover from the test assembly
        Sut.DiscoverFromCallingAssembly();
    }

    [Fact]
    public void ItShouldDiscoverTemplatesFromTestAssembly()
    {
        // The test assembly contains ComponentTemplate and CSharpClassTemplate
        Sut.GetNames().Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void ItShouldDiscoverComponentTemplate()
    {
        Sut.Get("Component").Should().NotBeNull();
    }
}

/// <summary>
/// Tests that discovery doesn't duplicate existing registrations.
/// </summary>
public class WhenDiscoveringAfterManualRegistration : WhenTestingFor<TemplateRegistry>
{
    private ComponentTemplate _manualTemplate = null!;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        _manualTemplate = new ComponentTemplate();
        registry.Register("Component", _manualTemplate);
        return registry;
    }

    protected override void When()
    {
        // Discover will overwrite existing registration with same name
        Sut.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
    }

    [Fact]
    public void ItShouldOverwriteManualRegistration()
    {
        // Discovery creates a new instance, so it won't be the same reference
        var discovered = Sut.Get("Component");
        discovered.Should().NotBeNull();
        // It should be a different instance (discovery creates new)
        discovered.Should().NotBeSameAs(_manualTemplate);
    }

    [Fact]
    public void ItShouldStillHaveOnlyOneComponentRegistration()
    {
        var names = Sut.GetNames().Where(n => n == "Component").ToList();
        names.Should().HaveCount(1);
    }
}

/// <summary>
/// Tests ValidateAll on discovered templates.
/// </summary>
public class WhenValidatingDiscoveredTemplates : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
        return registry;
    }

    protected override void When()
    {
        _validationResults = Sut.ValidateAll();
    }

    [Fact]
    public void ItShouldValidateAllDiscoveredTemplates()
    {
        _validationResults.Should().NotBeEmpty();
    }

    [Fact]
    public void ItShouldHaveValidComponentTemplate()
    {
        _validationResults.Should().ContainKey("Component");
        _validationResults!["Component"].IsValid.Should().BeTrue();
    }

    [Fact]
    public void ItShouldHaveValidCSharpClassTemplate()
    {
        _validationResults.Should().ContainKey("CSharpClass");
        _validationResults!["CSharpClass"].IsValid.Should().BeTrue();
    }
}

/// <summary>
/// Tests discovery with multiple calls to same assembly.
/// </summary>
public class WhenDiscoveringFromSameAssemblyMultipleTimes : WhenTestingFor<TemplateRegistry>
{
    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    protected override void When()
    {
        var assembly = typeof(ComponentTemplate).Assembly;
        Sut.DiscoverFromAssembly(assembly);
        Sut.DiscoverFromAssembly(assembly); // Discover again
    }

    [Fact]
    public void ItShouldNotDuplicateTemplates()
    {
        var componentCount = Sut.GetNames().Count(n => n == "Component");
        componentCount.Should().Be(1);
    }

    [Fact]
    public void ItShouldStillHaveAllTemplates()
    {
        Sut.Get("Component").Should().NotBeNull();
        Sut.Get("CSharpClass").Should().NotBeNull();
    }
}
