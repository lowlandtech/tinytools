using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "10")]
[UserStory("02", "Template engine renders negated conditions")]
public sealed class SC10_RendersNegatedConditions : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;

    public SC10_RendersNegatedConditions(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each negated template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("IsActive", false);
        useCase.Cases["SimpleNegation"] = Sut.Render("@if(!Context.IsActive) {\nInactive\n}", _context);
        useCase.Cases["SpacedNegation"] = Sut.Render("@if (!Context.IsActive) {\nNot Active\n}", _context);
        _context.Set("Count", 5);
        useCase.Cases["TruthyNegation"] = Sut.Render("@if(!Context.Count) {\nNo Count\n} else {\nHas Count\n}", _context);
        _context.Set("Count", 0);
        useCase.Cases["ZeroNegation"] = Sut.Render("@if(!Context.Count) {\nZero is falsy\n}", _context);
        _context.Set("Name", "");
        useCase.Cases["EmptyStringNegation"] = Sut.Render("@if(!Context.Name) {\nEmpty Name\n}", _context);
        useCase.Cases["NullNegation"] = Sut.Render("@if(!Context.Missing) {\nMissing is falsy\n}", _context);
    }

    [Then("the simple negation render should contain Inactive")]
    [Trait("UAC", "50"), Fact]
    public void UAC50_HandlesSimpleNegation() => ((string)useCase.Cases["SimpleNegation"]!).Should().Contain("Inactive");

    [Then("the spaced negation render should contain Not Active")]
    [Trait("UAC", "51"), Fact]
    public void UAC51_HandlesNegationWithoutParentheses() => ((string)useCase.Cases["SpacedNegation"]!).Should().Contain("Not Active");

    [Then("the negated truthy render should contain Has Count")]
    [Trait("UAC", "52"), Fact]
    public void UAC52_HandlesNegationOfTruthyValue() => ((string)useCase.Cases["TruthyNegation"]!).Should().Contain("Has Count").And.NotContain("No Count");

    [Then("the negated zero render should contain Zero is falsy")]
    [Trait("UAC", "53"), Fact]
    public void UAC53_HandlesNegationOfZero() => ((string)useCase.Cases["ZeroNegation"]!).Should().Contain("Zero is falsy");

    [Then("the negated empty string render should contain Empty Name")]
    [Trait("UAC", "54"), Fact]
    public void UAC54_HandlesNegationOfEmptyString() => ((string)useCase.Cases["EmptyStringNegation"]!).Should().Contain("Empty Name");

    [Then("the negated missing value render should contain Missing is falsy")]
    [Trait("UAC", "55"), Fact]
    public void UAC55_HandlesNegationOfNull() => ((string)useCase.Cases["NullNegation"]!).Should().Contain("Missing is falsy");
}
