using FluentAssertions;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests TemplateValidationResult factory methods.
/// </summary>
public class WhenCreatingValidationResults : WhenTestingFor<TemplateResult>
{
    private TemplateResult? _templateResult;

    protected override TemplateResult For()
    {
        return new TemplateResult
        {
            Content = "test content",
            Path = "test.txt",
            Namespace = "Test"
        };
    }

    protected override void Given()
    {
        _templateResult = Sut;
    }

    [Fact]
    public void ItShouldCreateSuccessResult()
    {
        var result = TemplateValidationResult.Success(_templateResult!);
        
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();
        result.ActualResult.Should().BeSameAs(_templateResult);
        result.Differences.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ItShouldCreateFailureResult()
    {
        var result = TemplateValidationResult.Failure("Test error");
        
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Test error");
        result.ActualResult.Should().BeNull();
    }

    [Fact]
    public void ItShouldCreateFailureResultWithActual()
    {
        var result = TemplateValidationResult.Failure("Test error", _templateResult);
        
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Test error");
        result.ActualResult.Should().BeSameAs(_templateResult);
    }

    [Fact]
    public void ItShouldCreateMismatchResult()
    {
        var differences = new Dictionary<string, (string Expected, string Actual)>
        {
            ["Content"] = ("Expected", "Actual"),
            ["Path"] = ("expected.txt", "actual.txt")
        };

        var result = TemplateValidationResult.Mismatch(_templateResult!, differences);
        
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("2 mismatch");
        result.ActualResult.Should().BeSameAs(_templateResult);
        result.Differences.Should().HaveCount(2);
        result.Differences!["Content"].Expected.Should().Be("Expected");
        result.Differences["Content"].Actual.Should().Be("Actual");
    }
}
