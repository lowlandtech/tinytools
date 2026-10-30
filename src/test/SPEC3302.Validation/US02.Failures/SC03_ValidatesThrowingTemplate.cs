namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US02.Failures;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "03")]
[UserStory("02", "Template validation handles failure scenarios")]
public sealed class SC03_ValidatesThrowingTemplate : TinyToolsScenario<ExceptionThrowingTemplate>, IClassFixture<UseCases.FailuresUseCase>
{
    private readonly UseCases.FailuresUseCase useCase;

    public SC03_ValidatesThrowingTemplate(UseCases.FailuresUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override ExceptionThrowingTemplate For() => new();

    [Given("a template whose test data throws during deserialization")]
    protected override void Given()
    {
    }

    [When("the template is validated with detail")]
    protected override void When()
    {
        base.When();

        useCase.Cases["Threw"] = false;
        try
        {
            useCase.Cases["Result"] = Sut.ValidateDetailed();
        }
        catch (Exception)
        {
            useCase.Cases["Threw"] = true;
        }
    }

    [Then("the result should be rejected")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReturnsInvalidResult() => ((TemplateValidationResult)useCase.Cases["Result"]!).IsValid.Should().BeFalse();

    [Then("the error message should contain Validation threw exception:")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasExceptionErrorMessage() => ((TemplateValidationResult)useCase.Cases["Result"]!).ErrorMessage.Should().Contain("Validation threw exception:");

    [Then("the actual result should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasNoActualResult() => ((TemplateValidationResult)useCase.Cases["Result"]!).ActualResult.Should().BeNull();

    [Then("the validation should be safe")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DoesNotThrowException() => useCase.Cases["Threw"].Should().Be(false);
}
