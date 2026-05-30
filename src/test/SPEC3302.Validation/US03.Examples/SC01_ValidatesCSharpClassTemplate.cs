namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US03.Examples;

/// <summary>
/// Tests template self-validation with CSharpClassTemplate.
/// </summary>
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template validation validates example templates")]
public class WhenValidatingCSharpClassTemplate : TinyToolsScenario<CSharpClassTemplate>
{
    private bool _validationResult;
    private TemplateValidationResult? _detailedResult;

    protected override CSharpClassTemplate For()
    {
        return new CSharpClassTemplate();
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
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have No Errors")]
    [Fact]
    public void ItShouldHaveNoErrors()
    {
        ArrangeAndAct();
        _detailedResult!.ErrorMessage.Should().BeNullOrEmpty();
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
