namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template validation checks all templates in registry")]
public sealed class SC01_ValidatesAllTemplates : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult> _results = [];

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    [Given("a registry holding the component and class templates")]
    protected override void Given()
    {
    }

    [When("every registered template is validated")]
    protected override void When()
    {
        base.When();
        _results = Sut.ValidateAll();
    }

    public SC01_ValidatesAllTemplates() => ArrangeAndAct();

    [Then("the validated templates should have count 2")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesAllTemplates() => _results.Should().HaveCount(2);

    [Then("the component validation should be accepted")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasComponentValidation() => new { Has = _results.ContainsKey("component"), Valid = _results["component"].IsValid }.Should().BeEquivalentTo(new { Has = true, Valid = true });

    [Then("the class validation should be accepted")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasClassValidation() => new { Has = _results.ContainsKey("class"), Valid = _results["class"].IsValid }.Should().BeEquivalentTo(new { Has = true, Valid = true });

    [Then("the validation errors should be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasNoErrors() => _results.Values.Should().OnlyContain(result => string.IsNullOrEmpty(result.ErrorMessage));
}
