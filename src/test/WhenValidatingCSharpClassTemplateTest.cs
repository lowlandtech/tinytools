using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests template self-validation with CSharpClassTemplate.
/// </summary>
public class WhenValidatingCSharpClassTemplate : WhenTestingFor<CSharpClassTemplate>
{
    private bool _validationResult;
    private TemplateValidationResult? _detailedResult;

    protected override CSharpClassTemplate For()
    {
        return new CSharpClassTemplate();
    }

    protected override void When()
    {
        _validationResult = Sut.Validate();
        _detailedResult = Sut.ValidateDetailed();
    }

    [Fact]
    public void ItShouldPassValidation()
    {
        _validationResult.Should().BeTrue();
    }

    [Fact]
    public void ItShouldHaveValidDetailedResult()
    {
        _detailedResult.Should().NotBeNull();
        _detailedResult!.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ItShouldHaveNoErrors()
    {
        _detailedResult!.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ItShouldHaveNoDifferences()
    {
        _detailedResult!.Differences.Should().BeNullOrEmpty();
    }
}
