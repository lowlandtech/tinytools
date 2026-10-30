using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "09")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC09_EvaluatesParenthesizedAndShortCircuitLogic : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context37 = new();
    private readonly ExecutionContext _context38 = new();
    private readonly ExecutionContext _context39 = new();
    private readonly ExecutionContext _context40 = new();

    public SC09_EvaluatesParenthesizedAndShortCircuitLogic(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with grouped flags, a pending status, no value and a default")]
    protected override void Given()
    {
        _context37.Set("A", true);
        _context37.Set("B", false);
        _context37.Set("C", true);
        _context38.Set("Status", "pending");
        _context39.Set("HasValue", false);
        _context40.Set("HasDefault", true);
    }

    [When("each parenthesized and short circuit template is rendered once")]
    protected override void When()
    {
        base.When();

        useCase.Cases["Parenthesized"] = Sut.Render("""
            @if ((Context.A && Context.B) || Context.C) {
            Result True
            }
            """, _context37);

        useCase.Cases["MultipleOr"] = Sut.Render("""
            @if (Context.Status == "active" || Context.Status == "pending" || Context.Status == "approved") {
            In Progress
            }
            """, _context38);

        useCase.Cases["ShortCircuitAnd"] = Sut.Render("""
            @if (Context.HasValue && Context.Value) {
            Has Value
            } else {
            No Value
            }
            """, _context39);

        useCase.Cases["ShortCircuitOr"] = Sut.Render("""
            @if (Context.HasDefault || Context.ComputedValue) {
            Has Something
            }
            """, _context40);
    }

    [Then("the render should contain Result True")]
    [Trait("UAC", "37"), Fact]
    public void UAC37_HandlesParenthesizedLogicalExpressions() => ((string)useCase.Cases["Parenthesized"]!).Should().Contain("Result True");

    [Then("the render should contain In Progress")]
    [Trait("UAC", "38"), Fact]
    public void UAC38_HandlesMultipleOrConditions() => ((string)useCase.Cases["MultipleOr"]!).Should().Contain("In Progress");

    [Then("the render should contain No Value")]
    [Trait("UAC", "39"), Fact]
    public void UAC39_ShortCircuitsLogicalAnd() => ((string)useCase.Cases["ShortCircuitAnd"]!).Should().Contain("No Value");

    [Then("the render should contain Has Something")]
    [Trait("UAC", "40"), Fact]
    public void UAC40_ShortCircuitsLogicalOr() => ((string)useCase.Cases["ShortCircuitOr"]!).Should().Contain("Has Something");
}
