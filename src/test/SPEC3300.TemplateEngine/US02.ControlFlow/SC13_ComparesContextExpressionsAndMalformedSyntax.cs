using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "13")]
[UserStory("02", "Template engine compares context expressions and preserves malformed syntax")]
public sealed class SC13_ComparesContextExpressionsAndMalformedSyntax : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;

    public SC13_ComparesContextExpressionsAndMalformedSyntax(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each context expression and malformed template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("A", 10);
        _context.Set("B", 5);
        useCase.Cases["AGreaterThanB"] = Sut.Render("@if (Context.A > Context.B) {\nA Greater Than B\n}", _context);
        _context.Set("X", "hello");
        _context.Set("Y", "hello");
        useCase.Cases["SameValue"] = Sut.Render("@if (Context.X == Context.Y) {\nSame Value\n}", _context);
        _context.Set("Y", "world");
        useCase.Cases["DifferentValues"] = Sut.Render("@if (Context.X != Context.Y) {\nDifferent Values\n}", _context);
        _context.Set("Score", 95);
        useCase.Cases["GradeA"] = Sut.Render("@if (Context.Score >= 90) {\nGrade A\n}", _context);
        _context.Set("Min", 1);
        _context.Set("Max", 100);
        useCase.Cases["MinLessThanMax"] = Sut.Render("@if (Context.Min < Context.Max) {\nMin Less Than Max\n}", _context);
        _context.Set("Value1", 5);
        _context.Set("Value2", 5);
        useCase.Cases["LessOrEqual"] = Sut.Render("@if (Context.Value1 <= Context.Value2) {\nLess Or Equal\n}", _context);
        _context.Set("Current", 10);
        _context.Set("Previous", 8);
        useCase.Cases["CurrentGreaterOrEqual"] = Sut.Render("@if (Context.Current >= Context.Previous) {\nCurrent Greater Or Equal\n}", _context);
        useCase.Cases["MalformedForeach"] = Sut.Render("@foreach {\nThis is malformed\n}\nNormal text", _context);
        useCase.Cases["MalformedIf"] = Sut.Render("@if {\nThis is malformed\n}\nNormal text", _context);
    }

    [Then("the context expression comparison should contain A Greater Than B")]
    [Trait("UAC", "77"), Fact]
    public void UAC77_ComparesContextExpressionOnRightSide() => ((string)useCase.Cases["AGreaterThanB"]!).Should().Contain("A Greater Than B");

    [Then("the equal context expressions render should contain Same Value")]
    [Trait("UAC", "78"), Fact]
    public void UAC78_ComparesTwoContextExpressionsEquality() => ((string)useCase.Cases["SameValue"]!).Should().Contain("Same Value");

    [Then("the unequal context expressions render should contain Different Values")]
    [Trait("UAC", "79"), Fact]
    public void UAC79_ComparesTwoContextExpressionsInequality() => ((string)useCase.Cases["DifferentValues"]!).Should().Contain("Different Values");

    [Then("the literal right side comparison should contain Grade A")]
    [Trait("UAC", "80"), Fact]
    public void UAC80_HandlesLiteralOnRightSideInComparison() => ((string)useCase.Cases["GradeA"]!).Should().Contain("Grade A");

    [Then("the context less-than comparison should contain Min Less Than Max")]
    [Trait("UAC", "81"), Fact]
    public void UAC81_ComparesTwoContextExpressionsLessThan() => ((string)useCase.Cases["MinLessThanMax"]!).Should().Contain("Min Less Than Max");

    [Then("the context less-or-equal comparison should contain Less Or Equal")]
    [Trait("UAC", "82"), Fact]
    public void UAC82_ComparesTwoContextExpressionsLessThanOrEqual() => ((string)useCase.Cases["LessOrEqual"]!).Should().Contain("Less Or Equal");

    [Then("the context greater-or-equal comparison should contain Current Greater Or Equal")]
    [Trait("UAC", "83"), Fact]
    public void UAC83_ComparesTwoContextExpressionsGreaterThanOrEqual() => ((string)useCase.Cases["CurrentGreaterOrEqual"]!).Should().Contain("Current Greater Or Equal");

    [Then("the malformed foreach render should contain the preserved lines")]
    [Trait("UAC", "84"), Fact]
    public void UAC84_HandlesMalformedForeachStatement() => ((string)useCase.Cases["MalformedForeach"]!).Should().Contain("@foreach {").And.Contain("Normal text");

    [Then("the malformed if render should contain the preserved lines")]
    [Trait("UAC", "85"), Fact]
    public void UAC85_HandlesMalformedIfStatement() => ((string)useCase.Cases["MalformedIf"]!).Should().Contain("@if {").And.Contain("Normal text");
}
