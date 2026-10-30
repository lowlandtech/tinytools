namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template validation checks all templates in registry")]
public sealed class SC02_HandlesPartialExpectations : TinyToolsScenario<PartialExpectationTemplate>, IClassFixture<UseCases.ValidationUseCase>
{
    private readonly UseCases.ValidationUseCase useCase;

    public SC02_HandlesPartialExpectations(UseCases.ValidationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override PartialExpectationTemplate For() => new();

    [Given("a template whose path and namespace expectations are absent")]
    protected override void Given()
    {
    }

    [When("the template is validated with detail")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Result"] = Sut.ValidateDetailed();
    }

    [Then("the content only validation should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesContentOnly() => new { IsValid = ((TemplateValidationResult)useCase.Cases["Result"]!).IsValid, HasDifferences = ((TemplateValidationResult)useCase.Cases["Result"]!).Differences is { Count: > 0 } }.Should().BeEquivalentTo(new { IsValid = true, HasDifferences = false });

    [Then("the differences should be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DoesNotValidatePathOrNamespace() => ((TemplateValidationResult)useCase.Cases["Result"]!).Differences.Should().BeNullOrEmpty();
}
