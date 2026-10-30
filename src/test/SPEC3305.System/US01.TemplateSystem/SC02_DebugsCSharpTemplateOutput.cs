namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC02_DebugsCSharpTemplateOutput : TinyToolsScenario<CSharpClassTemplate>
{
    private TemplateValidationResult _validation = null!;

    protected override CSharpClassTemplate For() => new();

    [Given("the example C# class template")]
    protected override void Given()
    {
    }

    [When("the template is validated with detail")]
    protected override void When()
    {
        base.When();
        _validation = Sut.ValidateDetailed();
    }

    public SC02_DebugsCSharpTemplateOutput() => ArrangeAndAct();

    [Then("the validation should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesTheCSharpTemplate() => _validation.IsValid.Should().BeTrue(_validation.ErrorMessage ?? "Validation should pass");
}
