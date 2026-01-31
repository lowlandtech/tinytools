using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;
using System.Text.Json;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Debug test to see actual template output vs expected
/// </summary>
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

