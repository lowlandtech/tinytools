namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "07")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC07_ValidatesDiscoveredTemplates : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult> _validationResults = [];

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.DiscoverFromAssembly(typeof(ComponentTemplate).Assembly);
        return registry;
    }

    [Given("a registry that discovered the templates of the test assembly")]
    protected override void Given()
    {
    }

    [When("every discovered template is validated")]
    protected override void When()
    {
        base.When();

        _validationResults = Sut.ValidateAll();
    }

    public SC07_ValidatesDiscoveredTemplates() => ArrangeAndAct();

    [Then("the validation results should contain at least one template")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesAllDiscoveredTemplates() => _validationResults.Should().NotBeEmpty();

    [Then("the validation results should contain the Component template")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsComponentTemplate() => _validationResults.Should().ContainKey("Component");

    [Then("the Component validation should be true")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasValidComponentTemplate() => _validationResults["Component"].IsValid.Should().BeTrue();

    [Then("the validation results should contain the CSharpClass template")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ContainsCSharpClassTemplate() => _validationResults.Should().ContainKey("CSharpClass");

    [Then("the CSharpClass validation should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_HasValidCSharpClassTemplate() => _validationResults["CSharpClass"].IsValid.Should().BeTrue();
}
