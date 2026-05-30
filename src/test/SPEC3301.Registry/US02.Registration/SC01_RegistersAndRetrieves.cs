namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US02.Registration;

/// <summary>
/// Tests TemplateRegistry registration functionality.
/// </summary>
[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template registry supports manual registration")]
public class WhenRegisteringTemplatesInRegistry : TinyToolsScenario<TemplateRegistry>
{
    private ComponentTemplate? _componentTemplate;
    private CSharpClassTemplate? _classTemplate;

    protected override TemplateRegistry For()
    {
        return new TemplateRegistry();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _componentTemplate = new ComponentTemplate();
        _classTemplate = new CSharpClassTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        Sut.Register("component", _componentTemplate);
        Sut.Register("class", _classTemplate);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Retrieve Component Template")]
    [Fact]
    public void ItShouldRetrieveComponentTemplate()
    {
        ArrangeAndAct();
        var retrieved = Sut.Get("component");
        retrieved.Should().NotBeNull();
        retrieved.Should().BeSameAs(_componentTemplate);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Retrieve C Sharp Class Template")]
    [Fact]
    public void ItShouldRetrieveCSharpClassTemplate()
    {
        ArrangeAndAct();
        var retrieved = Sut.Get("class");
        retrieved.Should().NotBeNull();
        retrieved.Should().BeSameAs(_classTemplate);
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Return Null For Unregistered Template")]
    [Fact]
    public void ItShouldReturnNullForUnregisteredTemplate()
    {
        ArrangeAndAct();
        var retrieved = Sut.Get("nonexistent");
        retrieved.Should().BeNull();
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should List All Registered Names")]
    [Fact]
    public void ItShouldListAllRegisteredNames()
    {
        ArrangeAndAct();
        var names = Sut.GetNames().ToList();
        names.Should().Contain("component");
        names.Should().Contain("class");
        names.Should().HaveCount(2);
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Get All Templates")]
    [Fact]
    public void ItShouldGetAllTemplates()
    {
        ArrangeAndAct();
        var templates = Sut.GetAll().ToList();
        templates.Should().HaveCount(2);
        templates.Should().Contain(_componentTemplate);
        templates.Should().Contain(_classTemplate);
    }
}
