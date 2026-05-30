namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US03.Examples;

/// <summary>
/// Tests template self-validation with ComponentTemplate.
/// </summary>
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "02")]
[UserStory("03", "Template validation validates example templates")]
public class WhenValidatingComponentTemplate : TinyToolsScenario<ComponentTemplate>
{
    private bool _validationResult;
    private TemplateValidationResult? _detailedResult;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _validationResult = Sut.Validate();
        _detailedResult = Sut.ValidateDetailed();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Pass Validation")]
    [Fact]
    public void ItShouldPassValidation()
    {
        ArrangeAndAct();
        _validationResult.Should().BeTrue();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Have Valid Detailed Result")]
    [Fact]
    public void ItShouldHaveValidDetailedResult()
    {
        ArrangeAndAct();
        _detailedResult.Should().NotBeNull();
        _detailedResult!.IsValid.Should().BeTrue();
        _detailedResult.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have Actual Result")]
    [Fact]
    public void ItShouldHaveActualResult()
    {
        ArrangeAndAct();
        _detailedResult!.ActualResult.Should().NotBeNull();
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Have No Differences")]
    [Fact]
    public void ItShouldHaveNoDifferences()
    {
        ArrangeAndAct();
        _detailedResult!.Differences.Should().BeNullOrEmpty();
    }
}

