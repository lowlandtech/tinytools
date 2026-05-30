namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

/// <summary>
/// Debug test for ComponentTemplate output
/// </summary>
[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template system validates and renders example templates")]
public class WhenDebuggingComponentTemplateOutput : TinyToolsScenario<ComponentTemplate>
{
    private TemplateValidationResult? _validationResult;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _validationResult = Sut.ValidateDetailed();
    }

    [Trait(Spec.UAC, "01")]
    [Then("show Component Validation Result")]
    [Fact]
    public void ShowComponentValidationResult()
    {
        ArrangeAndAct();
        _validationResult.IsValid.Should().BeTrue(_validationResult.ErrorMessage ?? "Validation should pass");
    }
}
