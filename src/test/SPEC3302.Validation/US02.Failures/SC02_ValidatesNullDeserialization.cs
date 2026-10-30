namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US02.Failures;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "02")]
[UserStory("02", "Template validation handles failure scenarios")]
public sealed class SC02_ValidatesNullDeserialization : TinyToolsScenario<NullDeserializationTemplate>
{
    private TemplateValidationResult _result = null!;

    protected override NullDeserializationTemplate For() => new();

    [Given("a template whose test data deserializes to null")]
    protected override void Given()
    {
    }

    [When("the template is validated with detail")]
    protected override void When()
    {
        base.When();
        _result = Sut.ValidateDetailed();
    }

    public SC02_ValidatesNullDeserialization() => ArrangeAndAct();

    [Then("the result should be rejected")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReturnsInvalidResult() => _result.IsValid.Should().BeFalse();

    [Then("the error message should contain Failed to deserialize TestDataJson")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasDeserializationErrorMessage() => _result.ErrorMessage.Should().Contain("Failed to deserialize TestDataJson");

    [Then("the actual result should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasNoActualResult() => _result.ActualResult.Should().BeNull();
}
