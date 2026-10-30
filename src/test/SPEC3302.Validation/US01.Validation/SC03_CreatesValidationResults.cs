namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template validation checks all templates in registry")]
public sealed class SC03_CreatesValidationResults : TinyToolsScenario<TemplateResult>, IClassFixture<UseCases.ValidationUseCase>
{
    private readonly UseCases.ValidationUseCase useCase;
    private TemplateResult _templateResult = null!;

    public SC03_CreatesValidationResults(UseCases.ValidationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TemplateResult For() => new()
    {
        Content = "test content",
        Path = "test.txt",
        Namespace = "Test",
    };

    [Given("a rendered template result and one content difference set")]
    protected override void Given() => _templateResult = Sut;

    [When("each validation result factory is applied once")]
    protected override void When()
    {
        base.When();

        useCase.Cases["TemplateResult"] = _templateResult;
        useCase.Cases["Success"] = TemplateValidationResult.Success(_templateResult);
        useCase.Cases["Failure"] = TemplateValidationResult.Failure("Test error");
        useCase.Cases["FailureWithActual"] = TemplateValidationResult.Failure("Test error", _templateResult);
        useCase.Cases["Mismatch"] = TemplateValidationResult.Mismatch(
            _templateResult,
            new Dictionary<string, (string Expected, string Actual)>
            {
                ["Content"] = ("Expected", "Actual"),
                ["Path"] = ("expected.txt", "actual.txt"),
            });
    }

    [Then("the success result should be complete")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_CreatesSuccessResult() => new { IsValid = ((TemplateValidationResult)useCase.Cases["Success"]!).IsValid, NoMessage = string.IsNullOrEmpty(((TemplateValidationResult)useCase.Cases["Success"]!).ErrorMessage), Same = ReferenceEquals(((TemplateValidationResult)useCase.Cases["Success"]!).ActualResult, useCase.Cases["TemplateResult"]), NoDifferences = ((TemplateValidationResult)useCase.Cases["Success"]!).Differences is not { Count: > 0 } }.Should().BeEquivalentTo(new { IsValid = true, NoMessage = true, Same = true, NoDifferences = true });

    [Then("the failure result should be complete")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CreatesFailureResult() => new { IsValid = ((TemplateValidationResult)useCase.Cases["Failure"]!).IsValid, ErrorMessage = ((TemplateValidationResult)useCase.Cases["Failure"]!).ErrorMessage, NoActual = ((TemplateValidationResult)useCase.Cases["Failure"]!).ActualResult is null }.Should().BeEquivalentTo(new { IsValid = false, ErrorMessage = "Test error", NoActual = true });

    [Then("the failure result with actual should be complete")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_CreatesFailureResultWithActual() => new { IsValid = ((TemplateValidationResult)useCase.Cases["FailureWithActual"]!).IsValid, ErrorMessage = ((TemplateValidationResult)useCase.Cases["FailureWithActual"]!).ErrorMessage, Same = ReferenceEquals(((TemplateValidationResult)useCase.Cases["FailureWithActual"]!).ActualResult, useCase.Cases["TemplateResult"]) }.Should().BeEquivalentTo(new { IsValid = false, ErrorMessage = "Test error", Same = true });

    [Then("the mismatch result should be complete")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_CreatesMismatchResult() => new { IsValid = ((TemplateValidationResult)useCase.Cases["Mismatch"]!).IsValid, Count = ((TemplateValidationResult)useCase.Cases["Mismatch"]!).Differences?.Count, MessageOk = ((TemplateValidationResult)useCase.Cases["Mismatch"]!).ErrorMessage?.Contains("2 mismatch") == true, Same = ReferenceEquals(((TemplateValidationResult)useCase.Cases["Mismatch"]!).ActualResult, useCase.Cases["TemplateResult"]) }.Should().BeEquivalentTo(new { IsValid = false, Count = (int?)2, MessageOk = true, Same = true });
}
