namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US03.Examples;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template validation validates example templates")]
public sealed class SC01_ValidatesCSharpClassTemplate : TinyToolsScenario<CSharpClassTemplate>
{
    private bool _validation;
    private TemplateValidationResult _detailed = null!;

    protected override CSharpClassTemplate For() => new();

    [Given("the example C# class template")]
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

    public SC01_ValidatesCSharpClassTemplate() => ArrangeAndAct();

    [Then("the validation should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PassesValidation() => _validation.Should().BeTrue();

    [Then("the detailed validation should be accepted")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasValidDetailedResult() => new { Present = _detailed is not null, IsValid = _detailed!.IsValid }.Should().BeEquivalentTo(new { Present = true, IsValid = true });

    [Then("the validation error should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasNoErrors() => _detailed!.ErrorMessage.Should().BeNullOrEmpty();

    [Then("the validation differences should be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasNoDifferences() => _detailed!.Differences.Should().BeNullOrEmpty();
}
