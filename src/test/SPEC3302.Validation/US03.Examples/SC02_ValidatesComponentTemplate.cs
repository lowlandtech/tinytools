namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US03.Examples;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "02")]
[UserStory("03", "Template validation validates example templates")]
public sealed class SC02_ValidatesComponentTemplate : TinyToolsScenario<ComponentTemplate>, IClassFixture<UseCases.ExamplesUseCase>
{
    private readonly UseCases.ExamplesUseCase useCase;

    public SC02_ValidatesComponentTemplate(UseCases.ExamplesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override ComponentTemplate For() => new();

    [Given("the example component template")]
    protected override void Given()
    {
    }

    [When("the template is validated and then validated with detail")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Validation"] = Sut.Validate();
        useCase.Cases["Detailed"] = Sut.ValidateDetailed();
    }

    [Then("the validation should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PassesValidation() => useCase.Cases["Validation"].Should().Be(true);

    [Then("the detailed validation should be accepted")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasValidDetailedResult() => new { IsValid = ((TemplateValidationResult)useCase.Cases["Detailed"]!).IsValid, NoError = string.IsNullOrEmpty(((TemplateValidationResult)useCase.Cases["Detailed"]!).ErrorMessage) }.Should().BeEquivalentTo(new { IsValid = true, NoError = true });

    [Then("the detailed result should not be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasActualResult() => ((TemplateValidationResult)useCase.Cases["Detailed"]!).ActualResult.Should().NotBeNull();

    [Then("the validation differences should be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasNoDifferences() => ((TemplateValidationResult)useCase.Cases["Detailed"]!).Differences.Should().BeNullOrEmpty();
}
