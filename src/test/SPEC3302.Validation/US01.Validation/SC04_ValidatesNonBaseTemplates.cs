namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "04")]
[UserStory("01", "Template validation checks all templates in registry")]
public sealed class SC04_ValidatesNonBaseTemplates : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.ValidationUseCase>
{
    private readonly UseCases.ValidationUseCase useCase;

    public SC04_ValidatesNonBaseTemplates(UseCases.ValidationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("simple-valid", new SimpleNonBaseTemplate(isValid: true));
        registry.Register("simple-invalid", new SimpleNonBaseTemplate(isValid: false));
        return registry;
    }

    [Given("a registry holding one valid and one invalid non-base template")]
    protected override void Given()
    {
    }

    [When("every registered template is validated")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Results"] = Sut.ValidateAll();
    }

    [Then("the validated templates should have count 2")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesBothTemplates() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!).Should().HaveCount(2);

    [Then("the valid template should be accepted")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasValidResultForValidTemplate() => new { Has = ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!).ContainsKey("simple-valid"), Valid = ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!)["simple-valid"].IsValid }.Should().BeEquivalentTo(new { Has = true, Valid = true });

    [Then("the valid template error message should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasNoErrorMessageForValidTemplate() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!)["simple-valid"].ErrorMessage.Should().BeNull();

    [Then("the invalid template should be reported")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasInvalidResultForInvalidTemplate() => new { Has = ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!).ContainsKey("simple-invalid"), Valid = ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!)["simple-invalid"].IsValid }.Should().BeEquivalentTo(new { Has = true, Valid = false });

    [Then("the invalid template error message should be exact")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_HasErrorMessageForInvalidTemplate() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!)["simple-invalid"].ErrorMessage.Should().Be("Validation failed");
}
