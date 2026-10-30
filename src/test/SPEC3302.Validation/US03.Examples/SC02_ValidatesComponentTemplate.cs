namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US03.Examples;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "02")]
[UserStory("03", "Template validation validates example templates")]
public sealed class SC02_ValidatesComponentTemplate : TinyToolsScenario<ComponentTemplate>
{
    private bool _validation;
    private TemplateValidationResult _detailed = null!;

    protected override ComponentTemplate For() => new();

    [Given("the example component template")]
    protected override void Given()
    {
    }

    [When("the template is validated and then validated with detail")]
    protected override void When()
    {
        base.When();
        _validation = Sut.Validate();
        _detailed = Sut.ValidateDetailed();
    }

    public SC02_ValidatesComponentTemplate() => ArrangeAndAct();

    [Then("the validation should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PassesValidation() => _validation.Should().BeTrue();

    [Then("the detailed validation should be accepted")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasValidDetailedResult() => new { IsValid = _detailed!.IsValid, NoError = string.IsNullOrEmpty(_detailed.ErrorMessage) }.Should().BeEquivalentTo(new { IsValid = true, NoError = true });

    [Then("the detailed result should not be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasActualResult() => _detailed!.ActualResult.Should().NotBeNull();

    [Then("the validation differences should be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasNoDifferences() => _detailed!.Differences.Should().BeNullOrEmpty();
}
