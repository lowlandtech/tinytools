namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

/// <summary>
/// Tests TemplateValidationResult factory methods.
/// </summary>
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template validation checks all templates in registry")]
public class WhenCreatingValidationResults : TinyToolsScenario<TemplateResult>
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

    [Given("Setup test context")]
    protected override void Given()
    {
        _templateResult = Sut;
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Create Success Result")]
    [Fact]
    public void ItShouldCreateSuccessResult()
    {
        ArrangeAndAct();
        var result = TemplateValidationResult.Success(_templateResult!);
        
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();
        result.ActualResult.Should().BeSameAs(_templateResult);
        result.Differences.Should().BeNullOrEmpty();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Create Failure Result")]
    [Fact]
    public void ItShouldCreateFailureResult()
    {
        ArrangeAndAct();
        var result = TemplateValidationResult.Failure("Test error");
        
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Test error");
        result.ActualResult.Should().BeNull();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Create Failure Result With Actual")]
    [Fact]
    public void ItShouldCreateFailureResultWithActual()
    {
        ArrangeAndAct();
        var result = TemplateValidationResult.Failure("Test error", _templateResult);
        
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Test error");
        result.ActualResult.Should().BeSameAs(_templateResult);
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Create Mismatch Result")]
    [Fact]
    public void ItShouldCreateMismatchResult()
    {
        ArrangeAndAct();
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
