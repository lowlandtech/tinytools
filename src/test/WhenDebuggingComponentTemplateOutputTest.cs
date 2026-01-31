using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Debug test for ComponentTemplate output
/// </summary>
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
