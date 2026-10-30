using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "09")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC09_EvaluatesParenthesizedAndShortCircuitLogic : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context37 = new();
    private readonly ExecutionContext _context38 = new();
    private readonly ExecutionContext _context39 = new();
    private readonly ExecutionContext _context40 = new();
    private string _result37 = string.Empty;
    private string _result38 = string.Empty;
    private string _result39 = string.Empty;
    private string _result40 = string.Empty;

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

        _result37 = Sut.Render("""
            @if ((Context.A && Context.B) || Context.C) {
            Result True
            }
            """, _context37);

        _result38 = Sut.Render("""
            @if (Context.Status == "active" || Context.Status == "pending" || Context.Status == "approved") {
            In Progress
            }
            """, _context38);

        _result39 = Sut.Render("""
            @if (Context.HasValue && Context.Value) {
            Has Value
            } else {
            No Value
            }
            """, _context39);

        _result40 = Sut.Render("""
            @if (Context.HasDefault || Context.ComputedValue) {
            Has Something
            }
            """, _context40);
    }

    public SC09_EvaluatesParenthesizedAndShortCircuitLogic() => ArrangeAndAct();

    [Then("the render should contain Result True")]
    [Trait("UAC", "37"), Fact]
    public void UAC37_HandlesParenthesizedLogicalExpressions() => _result37.Should().Contain("Result True");

    [Then("the render should contain In Progress")]
    [Trait("UAC", "38"), Fact]
    public void UAC38_HandlesMultipleOrConditions() => _result38.Should().Contain("In Progress");

    [Then("the render should contain No Value")]
    [Trait("UAC", "39"), Fact]
    public void UAC39_ShortCircuitsLogicalAnd() => _result39.Should().Contain("No Value");

    [Then("the render should contain Has Something")]
    [Trait("UAC", "40"), Fact]
    public void UAC40_ShortCircuitsLogicalOr() => _result40.Should().Contain("Has Something");
}
