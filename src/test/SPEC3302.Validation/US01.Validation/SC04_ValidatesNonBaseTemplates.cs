namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "04")]
[UserStory("01", "Template validation checks all templates in registry")]
public sealed class SC04_ValidatesNonBaseTemplates : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult> _results = [];

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
        _results = Sut.ValidateAll();
    }

    public SC04_ValidatesNonBaseTemplates() => ArrangeAndAct();

    [Then("the validated templates should have count 2")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesBothTemplates() => _results.Should().HaveCount(2);

    [Then("the valid template should be accepted")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasValidResultForValidTemplate() => new { Has = _results.ContainsKey("simple-valid"), Valid = _results["simple-valid"].IsValid }.Should().BeEquivalentTo(new { Has = true, Valid = true });

    [Then("the valid template error message should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasNoErrorMessageForValidTemplate() => _results["simple-valid"].ErrorMessage.Should().BeNull();

    [Then("the invalid template should be reported")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasInvalidResultForInvalidTemplate() => new { Has = _results.ContainsKey("simple-invalid"), Valid = _results["simple-invalid"].IsValid }.Should().BeEquivalentTo(new { Has = true, Valid = false });

    [Then("the invalid template error message should be exact")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_HasErrorMessageForInvalidTemplate() => _results["simple-invalid"].ErrorMessage.Should().Be("Validation failed");
}
