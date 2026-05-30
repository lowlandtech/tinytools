namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

/// <summary>
/// Debug test to see actual template output vs expected
/// </summary>
[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template system validates and renders example templates")]
public class WhenDebuggingTemplateOutput : TinyToolsScenario<CSharpClassTemplate>
{
    private TemplateValidationResult? _validationResult;

    protected override CSharpClassTemplate For()
    {
        return new CSharpClassTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _validationResult = Sut.ValidateDetailed();
    }

    [Trait(Spec.UAC, "01")]
    [Then("show Validation Result")]
    [Fact]
    public void ShowValidationResult()
    {
        ArrangeAndAct();
        _validationResult.IsValid.Should().BeTrue(_validationResult.ErrorMessage ?? "Validation should pass");
    }
}

