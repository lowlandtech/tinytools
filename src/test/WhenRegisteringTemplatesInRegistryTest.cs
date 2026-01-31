using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests TemplateRegistry registration functionality.
/// </summary>
public class WhenRegisteringTemplatesInRegistry : WhenTestingFor<TemplateRegistry>
{
    private ComponentTemplate? _componentTemplate;
    private CSharpClassTemplate? _classTemplate;

    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    protected override void Given()
    {
        _componentTemplate = new ComponentTemplate();
        _classTemplate = new CSharpClassTemplate();
    }

    protected override void When()
    {
        Sut.Register("component", _componentTemplate);
        Sut.Register("class", _classTemplate);
    }

    [Fact]
    public void ItShouldRetrieveComponentTemplate()
    {
        var retrieved = Sut.Get("component");
        retrieved.Should().NotBeNull();
        retrieved.Should().BeSameAs(_componentTemplate);
    }

    [Fact]
    public void ItShouldRetrieveCSharpClassTemplate()
    {
        var retrieved = Sut.Get("class");
        retrieved.Should().NotBeNull();
        retrieved.Should().BeSameAs(_classTemplate);
    }

    [Fact]
    public void ItShouldReturnNullForUnregisteredTemplate()
    {
        var retrieved = Sut.Get("nonexistent");
        retrieved.Should().BeNull();
    }

    [Fact]
    public void ItShouldListAllRegisteredNames()
    {
        var names = Sut.GetNames().ToList();
        names.Should().Contain("component");
        names.Should().Contain("class");
        names.Should().HaveCount(2);
    }

    [Fact]
    public void ItShouldGetAllTemplates()
    {
        var templates = Sut.GetAll().ToList();
        templates.Should().HaveCount(2);
        templates.Should().Contain(_componentTemplate);
        templates.Should().Contain(_classTemplate);
    }
}
