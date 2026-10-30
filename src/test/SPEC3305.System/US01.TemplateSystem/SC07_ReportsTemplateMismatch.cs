namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "07")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC07_ReportsTemplateMismatch : TinyToolsScenario<TestFailingTemplate>
{
    private TemplateValidationResult _validation = null!;

    protected override TestFailingTemplate For() => new();

    [Given("a template whose expected content does not match")]
    protected override void Given()
    {
    }

    [When("the template is validated with detail")]
    protected override void When()
    {
        base.When();
        _validation = Sut.ValidateDetailed();
    }

    public SC07_ReportsTemplateMismatch() => ArrangeAndAct();

    [Then("the failing template validation should be false")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RejectsTheMismatchedTemplate() => _validation.IsValid.Should().BeFalse();

    [Then("the validation differences should not be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ReportsTheDifferences() => _validation.Differences.Should().NotBeNull();

    [Then("the validation differences should contain Content")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReportsTheContentDifference() => _validation.Differences!.Keys.Should().Contain("Content");
}
