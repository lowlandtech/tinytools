using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "13")]
[UserStory("02", "Template engine compares context expressions and preserves malformed syntax")]
public sealed class SC13_ComparesContextExpressionsAndMalformedSyntax : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _aGreaterThanB = string.Empty;
    private string _sameValue = string.Empty;
    private string _differentValues = string.Empty;
    private string _gradeA = string.Empty;
    private string _minLessThanMax = string.Empty;
    private string _lessOrEqual = string.Empty;
    private string _currentGreaterOrEqual = string.Empty;
    private string _malformedForeach = string.Empty;
    private string _malformedIf = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each context expression and malformed template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("A", 10);
        _context.Set("B", 5);
        _aGreaterThanB = Sut.Render("@if (Context.A > Context.B) {\nA Greater Than B\n}", _context);
        _context.Set("X", "hello");
        _context.Set("Y", "hello");
        _sameValue = Sut.Render("@if (Context.X == Context.Y) {\nSame Value\n}", _context);
        _context.Set("Y", "world");
        _differentValues = Sut.Render("@if (Context.X != Context.Y) {\nDifferent Values\n}", _context);
        _context.Set("Score", 95);
        _gradeA = Sut.Render("@if (Context.Score >= 90) {\nGrade A\n}", _context);
        _context.Set("Min", 1);
        _context.Set("Max", 100);
        _minLessThanMax = Sut.Render("@if (Context.Min < Context.Max) {\nMin Less Than Max\n}", _context);
        _context.Set("Value1", 5);
        _context.Set("Value2", 5);
        _lessOrEqual = Sut.Render("@if (Context.Value1 <= Context.Value2) {\nLess Or Equal\n}", _context);
        _context.Set("Current", 10);
        _context.Set("Previous", 8);
        _currentGreaterOrEqual = Sut.Render("@if (Context.Current >= Context.Previous) {\nCurrent Greater Or Equal\n}", _context);
        _malformedForeach = Sut.Render("@foreach {\nThis is malformed\n}\nNormal text", _context);
        _malformedIf = Sut.Render("@if {\nThis is malformed\n}\nNormal text", _context);
    }

    public SC13_ComparesContextExpressionsAndMalformedSyntax() => ArrangeAndAct();

    [Then("the context expression comparison should contain A Greater Than B")]
    [Trait("UAC", "77"), Fact]
    public void UAC77_ComparesContextExpressionOnRightSide() => _aGreaterThanB.Should().Contain("A Greater Than B");

    [Then("the equal context expressions render should contain Same Value")]
    [Trait("UAC", "78"), Fact]
    public void UAC78_ComparesTwoContextExpressionsEquality() => _sameValue.Should().Contain("Same Value");

    [Then("the unequal context expressions render should contain Different Values")]
    [Trait("UAC", "79"), Fact]
    public void UAC79_ComparesTwoContextExpressionsInequality() => _differentValues.Should().Contain("Different Values");

    [Then("the literal right side comparison should contain Grade A")]
    [Trait("UAC", "80"), Fact]
    public void UAC80_HandlesLiteralOnRightSideInComparison() => _gradeA.Should().Contain("Grade A");

    [Then("the context less-than comparison should contain Min Less Than Max")]
    [Trait("UAC", "81"), Fact]
    public void UAC81_ComparesTwoContextExpressionsLessThan() => _minLessThanMax.Should().Contain("Min Less Than Max");

    [Then("the context less-or-equal comparison should contain Less Or Equal")]
    [Trait("UAC", "82"), Fact]
    public void UAC82_ComparesTwoContextExpressionsLessThanOrEqual() => _lessOrEqual.Should().Contain("Less Or Equal");

    [Then("the context greater-or-equal comparison should contain Current Greater Or Equal")]
    [Trait("UAC", "83"), Fact]
    public void UAC83_ComparesTwoContextExpressionsGreaterThanOrEqual() => _currentGreaterOrEqual.Should().Contain("Current Greater Or Equal");

    [Then("the malformed foreach render should contain the preserved lines")]
    [Trait("UAC", "84"), Fact]
    public void UAC84_HandlesMalformedForeachStatement() => _malformedForeach.Should().Contain("@foreach {").And.Contain("Normal text");

    [Then("the malformed if render should contain the preserved lines")]
    [Trait("UAC", "85"), Fact]
    public void UAC85_HandlesMalformedIfStatement() => _malformedIf.Should().Contain("@if {").And.Contain("Normal text");
}
