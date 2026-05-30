using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

/// <summary>
/// Debug test to see actual template output vs expected
/// </summary>
[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template system validates and renders example templates")]
public class WhenDebuggingTemplateOutput : WhenTestingFor<CSharpClassTemplate>
{
    private TemplateValidationResult? _validationResult;

    protected override CSharpClassTemplate For()
    {
        return new CSharpClassTemplate();
    }

    protected override void When()
    {
        _validationResult = Sut.ValidateDetailed();
    }

    [Fact]
    public void ShowValidationResult()
    {
        _validationResult.IsValid.Should().BeTrue(_validationResult.ErrorMessage ?? "Validation should pass");
    }
}

