namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "07")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC07_ValidatesDiscoveredTemplates : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.DiscoveryUseCase>
{
    private readonly UseCases.DiscoveryUseCase useCase;

    public SC07_ValidatesDiscoveredTemplates(UseCases.DiscoveryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["ValidationResults"] = Sut.ValidateAll();
    }

    [Then("the validation results should contain at least one template")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesAllDiscoveredTemplates() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["ValidationResults"]!).Should().NotBeEmpty();

    [Then("the validation results should contain the Component template")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsComponentTemplate() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["ValidationResults"]!).Should().ContainKey("Component");

    [Then("the Component validation should be true")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasValidComponentTemplate() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["ValidationResults"]!)["Component"].IsValid.Should().BeTrue();

    [Then("the validation results should contain the CSharpClass template")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ContainsCSharpClassTemplate() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["ValidationResults"]!).Should().ContainKey("CSharpClass");

    [Then("the CSharpClass validation should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_HasValidCSharpClassTemplate() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["ValidationResults"]!)["CSharpClass"].IsValid.Should().BeTrue();
}
