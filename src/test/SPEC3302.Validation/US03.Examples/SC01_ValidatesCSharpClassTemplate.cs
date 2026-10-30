namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US03.Examples;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template validation validates example templates")]
public sealed class SC01_ValidatesCSharpClassTemplate : TinyToolsScenario<CSharpClassTemplate>, IClassFixture<UseCases.ExamplesUseCase>
{
    private readonly UseCases.ExamplesUseCase useCase;

    public SC01_ValidatesCSharpClassTemplate(UseCases.ExamplesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override CSharpClassTemplate For() => new();

    [Given("the example C# class template")]
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
    public void UAC02_HasValidDetailedResult() => new { Present = useCase.Cases["Detailed"] is not null, IsValid = ((TemplateValidationResult)useCase.Cases["Detailed"]!).IsValid }.Should().BeEquivalentTo(new { Present = true, IsValid = true });

    [Then("the validation error should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasNoErrors() => ((TemplateValidationResult)useCase.Cases["Detailed"]!).ErrorMessage.Should().BeNullOrEmpty();

    [Then("the validation differences should be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasNoDifferences() => ((TemplateValidationResult)useCase.Cases["Detailed"]!).Differences.Should().BeNullOrEmpty();
}
