using LowlandTech.TinyTools.Tests.Shared.Examples;

namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

/// <summary>
/// Debug test for ComponentTemplate output
/// </summary>
[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template system validates and renders example templates")]
public class WhenDebuggingComponentTemplateOutput : WhenTestingFor<ComponentTemplate>
{
    private TemplateValidationResult? _validationResult;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
    }

    protected override void When()
    {
        _validationResult = Sut.ValidateDetailed();
    }

    [Fact]
    public void ShowComponentValidationResult()
    {
        _validationResult.IsValid.Should().BeTrue(_validationResult.ErrorMessage ?? "Validation should pass");
    }
}
