namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC03_DebugsComponentTemplateOutput : TinyToolsScenario<ComponentTemplate>, IClassFixture<UseCases.TemplateSystemUseCase>
{
    private readonly UseCases.TemplateSystemUseCase useCase;

    public SC03_DebugsComponentTemplateOutput(UseCases.TemplateSystemUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override ComponentTemplate For() => new();

    [Given("the example component template")]
    protected override void Given()
    {
    }

    [When("the template is validated with detail")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Validation"] = Sut.ValidateDetailed();
    }

    [Then("the component validation should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesTheComponentTemplate() => ((TemplateValidationResult)useCase.Cases["Validation"]!).IsValid.Should().BeTrue(((TemplateValidationResult)useCase.Cases["Validation"]!).ErrorMessage ?? "Validation should pass");
}
