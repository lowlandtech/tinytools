namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US02.Failures;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template validation handles failure scenarios")]
public sealed class SC01_DetectsFailures : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult> _results = [];

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("valid", new ComponentTemplate());
        registry.Register("invalid", new InvalidTemplate());
        return registry;
    }

    [Given("a registry holding one valid and one deliberately mismatched template")]
    protected override void Given()
    {
    }

    [When("every registered template is validated")]
    protected override void When()
    {
        base.When();
        _results = Sut.ValidateAll();
    }

    public SC01_DetectsFailures() => ArrangeAndAct();

    [Then("the valid template should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_HasValidTemplatePass() => _results["valid"].IsValid.Should().BeTrue();

    [Then("the invalid template should be rejected")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasInvalidTemplateFail() => _results["invalid"].IsValid.Should().BeFalse();

    [Then("the invalid error message should not be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasErrorMessageForInvalidTemplate() => _results["invalid"].ErrorMessage.Should().NotBeNullOrEmpty();

    [Then("the invalid differences should not be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasDifferencesForInvalidTemplate() => _results["invalid"].Differences.Should().NotBeNullOrEmpty();
}
