using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "12")]
[UserStory("02", "Template engine compares mixed values and reports iteration errors")]
public sealed class SC12_ComparesMixedValuesAndReportsErrors : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;

    public SC12_ComparesMixedValuesAndReportsErrors(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each mixed comparison template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("Value", "something");
        useCase.Cases["NotNullValue"] = Sut.Render("@if (Context.Value != null) {\nNot Null\n}", _context);
        useCase.Cases["IsNull"] = Sut.Render("@if (Context.Missing == null) {\nIs Null\n}", _context);
        _context.Set("A", null);
        _context.Set("B", 5);
        useCase.Cases["NullLessThanValue"] = Sut.Render("@if (Context.A < Context.B) {\nNull Less Than Value\n}", _context);
        _context.Set("A", 5);
        useCase.Cases["ValueGreaterThanNull"] = Sut.Render("@if (Context.A > null) {\nValue Greater Than Null\n}", _context);
        _context.Set("A", "apple");
        _context.Set("B", "banana");
        useCase.Cases["StringOrder"] = Sut.Render("@if (Context.A < Context.B) {\nApple Before Banana\n}", _context);
        _context.Set("Name", "Hello");
        useCase.Cases["StringForeach"] = (Action)(() => Sut.Render("@foreach(var item in Context.Name) {\nItem: ${item}\n}", _context));
        _context.Set("Value1", 0.1 + 0.2);
        _context.Set("Value2", 0.3);
        useCase.Cases["TolerantEquality"] = Sut.Render("@if (Context.Value1 == Context.Value2) {\nValues Are Equal\n} else {\nValues Are Not Equal\n}", _context);
        _context.Set("Value1", 1.0);
        _context.Set("Value2", 1.0001);
        useCase.Cases["OutOfTolerance"] = Sut.Render("@if (Context.Value1 == Context.Value2) {\nValues Are Equal\n} else {\nValues Are Not Equal\n}", _context);
        _context.Set("IsActive", true);
        useCase.Cases["ParenthesisedNegation"] = Sut.Render("@if (!(Context.IsActive)) {\nNot Active\n} else {\nActive\n}", _context);
        _context.Set("Flag", false);
        useCase.Cases["NestedNegation"] = Sut.Render("@if (!(Context.Flag)) {\nFlag Is False\n}", _context);
    }

    [Then("the value against null literal render should contain Not Null")]
    [Trait("UAC", "67"), Fact]
    public void UAC67_ComparesValueWithNullLiteral() => ((string)useCase.Cases["NotNullValue"]!).Should().Contain("Not Null");

    [Then("the missing value against null render should contain Is Null")]
    [Trait("UAC", "68"), Fact]
    public void UAC68_ComparesMissingValueWithNull() => ((string)useCase.Cases["IsNull"]!).Should().Contain("Is Null");

    [Then("the null against value render should contain Null Less Than Value")]
    [Trait("UAC", "69"), Fact]
    public void UAC69_ComparesNullWithValue() => ((string)useCase.Cases["NullLessThanValue"]!).Should().Contain("Null Less Than Value");

    [Then("the value against null render should contain Value Greater Than Null")]
    [Trait("UAC", "70"), Fact]
    public void UAC70_ComparesValueWithNull() => ((string)useCase.Cases["ValueGreaterThanNull"]!).Should().Contain("Value Greater Than Null");

    [Then("the alphabetical comparison should contain Apple Before Banana")]
    [Trait("UAC", "71"), Fact]
    public void UAC71_ComparesStringsAlphabetically() => ((string)useCase.Cases["StringOrder"]!).Should().Contain("Apple Before Banana");

    [Then("the string iteration should fail with InvalidOperationException")]
    [Trait("UAC", "72"), Fact]
    public void UAC72_ThrowsWhenForeachIteratesOverString() => ((Action)useCase.Cases["StringForeach"]!).Should().Throw<InvalidOperationException>().WithMessage("*Cannot iterate over string value in @foreach*Context.Name*");

    [Then("the tolerance equality render should contain Values Are Equal")]
    [Trait("UAC", "73"), Fact]
    public void UAC73_HandlesFloatingPointEqualityWithTolerance() => ((string)useCase.Cases["TolerantEquality"]!).Should().Contain("Values Are Equal").And.NotContain("Values Are Not Equal");

    [Then("the out-of-tolerance render should contain Values Are Not Equal")]
    [Trait("UAC", "74"), Fact]
    public void UAC74_HandlesFloatingPointInequalityWithTolerance() => ((string)useCase.Cases["OutOfTolerance"]!).Should().Contain("Values Are Not Equal").And.NotContain("Values Are Equal");

    [Then("the parenthesised negation render should contain Active")]
    [Trait("UAC", "75"), Fact]
    public void UAC75_HandlesNegationWithParentheses() => ((string)useCase.Cases["ParenthesisedNegation"]!).Should().Contain("Active").And.NotContain("Not Active");

    [Then("the nested parenthesised negation render should contain Flag Is False")]
    [Trait("UAC", "76"), Fact]
    public void UAC76_HandlesNestedNegationWithParentheses() => ((string)useCase.Cases["NestedNegation"]!).Should().Contain("Flag Is False");
}
