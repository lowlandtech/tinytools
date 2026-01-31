using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests template self-validation with ComponentTemplate.
/// </summary>
public class WhenValidatingComponentTemplate : WhenTestingFor<ComponentTemplate>
{
    private bool _validationResult;
    private TemplateValidationResult? _detailedResult;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
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
        _detailedResult.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ItShouldHaveActualResult()
    {
        _detailedResult!.ActualResult.Should().NotBeNull();
    }

    [Fact]
    public void ItShouldHaveNoDifferences()
    {
        _detailedResult!.Differences.Should().BeNullOrEmpty();
    }
}

