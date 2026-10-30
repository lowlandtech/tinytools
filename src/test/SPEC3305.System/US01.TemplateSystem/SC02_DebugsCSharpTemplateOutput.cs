namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC02_DebugsCSharpTemplateOutput : TinyToolsScenario<CSharpClassTemplate>, IClassFixture<UseCases.TemplateSystemUseCase>
{
    private readonly UseCases.TemplateSystemUseCase useCase;

    public SC02_DebugsCSharpTemplateOutput(UseCases.TemplateSystemUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override CSharpClassTemplate For() => new();

    [Given("the example C# class template")]
    protected override void Given()
    {
    }

    [When("the template is validated with detail")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Validation"] = Sut.ValidateDetailed();
    }

    [Then("the validation should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesTheCSharpTemplate() => ((TemplateValidationResult)useCase.Cases["Validation"]!).IsValid.Should().BeTrue(((TemplateValidationResult)useCase.Cases["Validation"]!).ErrorMessage ?? "Validation should pass");
}
