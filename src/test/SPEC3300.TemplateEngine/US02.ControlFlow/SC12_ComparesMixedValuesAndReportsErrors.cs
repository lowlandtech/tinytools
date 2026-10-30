using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "12")]
[UserStory("02", "Template engine compares mixed values and reports iteration errors")]
public sealed class SC12_ComparesMixedValuesAndReportsErrors : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _notNullValue = string.Empty;
    private string _isNull = string.Empty;
    private string _nullLessThanValue = string.Empty;
    private string _valueGreaterThanNull = string.Empty;
    private string _stringOrder = string.Empty;
    private InvalidOperationException _stringForeachError = null!;
    private string _tolerantEquality = string.Empty;
    private string _outOfTolerance = string.Empty;
    private string _parenthesisedNegation = string.Empty;
    private string _nestedNegation = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each mixed comparison template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("Value", "something");
        _notNullValue = Sut.Render("@if (Context.Value != null) {\nNot Null\n}", _context);
        _isNull = Sut.Render("@if (Context.Missing == null) {\nIs Null\n}", _context);
        _context.Set("A", null);
        _context.Set("B", 5);
        _nullLessThanValue = Sut.Render("@if (Context.A < Context.B) {\nNull Less Than Value\n}", _context);
        _context.Set("A", 5);
        _valueGreaterThanNull = Sut.Render("@if (Context.A > null) {\nValue Greater Than Null\n}", _context);
        _context.Set("A", "apple");
        _context.Set("B", "banana");
        _stringOrder = Sut.Render("@if (Context.A < Context.B) {\nApple Before Banana\n}", _context);
        _context.Set("Name", "Hello");
        _stringForeachError = Assert.Throws<InvalidOperationException>(
            () => Sut.Render("@foreach(var item in Context.Name) {\nItem: ${item}\n}", _context));
        _context.Set("Value1", 0.1 + 0.2);
        _context.Set("Value2", 0.3);
        _tolerantEquality = Sut.Render("@if (Context.Value1 == Context.Value2) {\nValues Are Equal\n} else {\nValues Are Not Equal\n}", _context);
        _context.Set("Value1", 1.0);
        _context.Set("Value2", 1.0001);
        _outOfTolerance = Sut.Render("@if (Context.Value1 == Context.Value2) {\nValues Are Equal\n} else {\nValues Are Not Equal\n}", _context);
        _context.Set("IsActive", true);
        _parenthesisedNegation = Sut.Render("@if (!(Context.IsActive)) {\nNot Active\n} else {\nActive\n}", _context);
        _context.Set("Flag", false);
        _nestedNegation = Sut.Render("@if (!(Context.Flag)) {\nFlag Is False\n}", _context);
    }

    public SC12_ComparesMixedValuesAndReportsErrors() => ArrangeAndAct();

    [Then("the value against null literal render should contain Not Null")]
    [Trait("UAC", "67"), Fact]
    public void UAC67_ComparesValueWithNullLiteral() => _notNullValue.Should().Contain("Not Null");

    [Then("the missing value against null render should contain Is Null")]
    [Trait("UAC", "68"), Fact]
    public void UAC68_ComparesMissingValueWithNull() => _isNull.Should().Contain("Is Null");

    [Then("the null against value render should contain Null Less Than Value")]
    [Trait("UAC", "69"), Fact]
    public void UAC69_ComparesNullWithValue() => _nullLessThanValue.Should().Contain("Null Less Than Value");

    [Then("the value against null render should contain Value Greater Than Null")]
    [Trait("UAC", "70"), Fact]
    public void UAC70_ComparesValueWithNull() => _valueGreaterThanNull.Should().Contain("Value Greater Than Null");

    [Then("the alphabetical comparison should contain Apple Before Banana")]
    [Trait("UAC", "71"), Fact]
    public void UAC71_ComparesStringsAlphabetically() => _stringOrder.Should().Contain("Apple Before Banana");

    [Then("the string iteration should fail with InvalidOperationException")]
    [Trait("UAC", "72"), Fact]
    public void UAC72_ThrowsWhenForeachIteratesOverString() => _stringForeachError.Message.Should().Contain("Cannot iterate over string value in @foreach").And.Contain("Context.Name");

    [Then("the tolerance equality render should contain Values Are Equal")]
    [Trait("UAC", "73"), Fact]
    public void UAC73_HandlesFloatingPointEqualityWithTolerance() => _tolerantEquality.Should().Contain("Values Are Equal").And.NotContain("Values Are Not Equal");

    [Then("the out-of-tolerance render should contain Values Are Not Equal")]
    [Trait("UAC", "74"), Fact]
    public void UAC74_HandlesFloatingPointInequalityWithTolerance() => _outOfTolerance.Should().Contain("Values Are Not Equal").And.NotContain("Values Are Equal");

    [Then("the parenthesised negation render should contain Active")]
    [Trait("UAC", "75"), Fact]
    public void UAC75_HandlesNegationWithParentheses() => _parenthesisedNegation.Should().Contain("Active").And.NotContain("Not Active");

    [Then("the nested parenthesised negation render should contain Flag Is False")]
    [Trait("UAC", "76"), Fact]
    public void UAC76_HandlesNestedNegationWithParentheses() => _nestedNegation.Should().Contain("Flag Is False");
}
