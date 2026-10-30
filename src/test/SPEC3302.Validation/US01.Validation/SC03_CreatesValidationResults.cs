namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template validation checks all templates in registry")]
public sealed class SC03_CreatesValidationResults : TinyToolsScenario<TemplateResult>
{
    private TemplateResult _templateResult = null!;
    private TemplateValidationResult _success = null!;
    private TemplateValidationResult _failure = null!;
    private TemplateValidationResult _failureWithActual = null!;
    private TemplateValidationResult _mismatch = null!;

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

        _success = TemplateValidationResult.Success(_templateResult);
        _failure = TemplateValidationResult.Failure("Test error");
        _failureWithActual = TemplateValidationResult.Failure("Test error", _templateResult);
        _mismatch = TemplateValidationResult.Mismatch(
            _templateResult,
            new Dictionary<string, (string Expected, string Actual)>
            {
                ["Content"] = ("Expected", "Actual"),
                ["Path"] = ("expected.txt", "actual.txt"),
            });
    }

    public SC03_CreatesValidationResults() => ArrangeAndAct();

    [Then("the success result should be complete")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_CreatesSuccessResult() => new { _success.IsValid, NoMessage = string.IsNullOrEmpty(_success.ErrorMessage), Same = ReferenceEquals(_success.ActualResult, _templateResult), NoDifferences = _success.Differences is not { Count: > 0 } }.Should().BeEquivalentTo(new { IsValid = true, NoMessage = true, Same = true, NoDifferences = true });

    [Then("the failure result should be complete")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CreatesFailureResult() => new { _failure.IsValid, _failure.ErrorMessage, NoActual = _failure.ActualResult is null }.Should().BeEquivalentTo(new { IsValid = false, ErrorMessage = "Test error", NoActual = true });

    [Then("the failure result with actual should be complete")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_CreatesFailureResultWithActual() => new { _failureWithActual.IsValid, _failureWithActual.ErrorMessage, Same = ReferenceEquals(_failureWithActual.ActualResult, _templateResult) }.Should().BeEquivalentTo(new { IsValid = false, ErrorMessage = "Test error", Same = true });

    [Then("the mismatch result should be complete")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_CreatesMismatchResult() => new { _mismatch.IsValid, Count = _mismatch.Differences?.Count, MessageOk = _mismatch.ErrorMessage?.Contains("2 mismatch") == true, Same = ReferenceEquals(_mismatch.ActualResult, _templateResult) }.Should().BeEquivalentTo(new { IsValid = false, Count = (int?)2, MessageOk = true, Same = true });
}
