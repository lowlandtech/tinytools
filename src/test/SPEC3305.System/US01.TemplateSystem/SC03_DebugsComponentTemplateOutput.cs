namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC03_DebugsComponentTemplateOutput : TinyToolsScenario<ComponentTemplate>
{
    private TemplateValidationResult _validation = null!;

    protected override ComponentTemplate For() => new();

    [Given("the example component template")]
    protected override void Given()
    {
    }

    [When("the template is validated with detail")]
    protected override void When()
    {
        base.When();
        _validation = Sut.ValidateDetailed();
    }

    public SC03_DebugsComponentTemplateOutput() => ArrangeAndAct();

    [Then("the component validation should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesTheComponentTemplate() => _validation.IsValid.Should().BeTrue(_validation.ErrorMessage ?? "Validation should pass");
}
