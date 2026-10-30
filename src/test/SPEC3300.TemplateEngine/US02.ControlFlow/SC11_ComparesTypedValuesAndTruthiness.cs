using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "11")]
[UserStory("02", "Template engine compares typed values and evaluates truthiness")]
public sealed class SC11_ComparesTypedValuesAndTruthiness : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _doubleValue = string.Empty;
    private string _decimalValue = string.Empty;
    private string _booleanLiteral = string.Empty;
    private string _falseLiteral = string.Empty;
    private string _ageComparison = string.Empty;
    private string _notNullValue = string.Empty;
    private string _nonZeroInt = string.Empty;
    private string _emptyList = string.Empty;
    private string _trueFlag = string.Empty;
    private string _falseFlag = string.Empty;
    private string _object = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each typed comparison and truthiness template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("Value", 3.14);
        _doubleValue = Sut.Render("@if (Context.Value > 3) {\nGreater\n}", _context);
        _context.Set("Value", 99.99m);
        _decimalValue = Sut.Render("@if (Context.Value >= 100) {\nExpensive\n} else {\nAffordable\n}", _context);
        _context.Set("Flag", true);
        _booleanLiteral = Sut.Render("@if (Context.Flag == true) {\nIs True\n}", _context);
        _trueFlag = Sut.Render("@if (Context.Flag) {\nTrue\n}", _context);
        _context.Set("Flag", false);
        _falseLiteral = Sut.Render("@if (Context.Flag == false) {\nIs False\n}", _context);
        _falseFlag = Sut.Render("@if (Context.Flag) {\nTrue\n} else {\nFalse\n}", _context);
        _context.Set("Age", 17);
        _ageComparison = Sut.Render("@if (Context.Age < 18) {\nMinor\n}", _context);
        _context.Set("Value", "something");
        _notNullValue = Sut.Render("@if (Context.Value != null) {\nHas Value\n}", _context);
        _context.Set("Value", -1);
        _nonZeroInt = Sut.Render("@if (Context.Value) {\nTruthy\n}", _context);
        _context.Set("Items", new List<string>());
        _emptyList = Sut.Render("@if (Context.Items) {\nHas Items\n} else {\nNo Items\n}", _context);
        _context.Set("Obj", new { Name = "Test" });
        _object = Sut.Render("@if (Context.Obj) {\nHas Object\n}", _context);
    }

    public SC11_ComparesTypedValuesAndTruthiness() => ArrangeAndAct();

    [Then("the double value comparison should contain Greater")]
    [Trait("UAC", "56"), Fact]
    public void UAC56_HandlesNumericComparisonWithDoubles() => _doubleValue.Should().Contain("Greater");

    [Then("the decimal value comparison should contain Affordable")]
    [Trait("UAC", "57"), Fact]
    public void UAC57_HandlesNumericComparisonWithDecimals() => _decimalValue.Should().Contain("Affordable");

    [Then("the true literal comparison should contain Is True")]
    [Trait("UAC", "58"), Fact]
    public void UAC58_HandlesComparisonWithBooleanLiteral() => _booleanLiteral.Should().Contain("Is True");

    [Then("the false literal comparison should contain Is False")]
    [Trait("UAC", "59"), Fact]
    public void UAC59_HandlesComparisonWithFalseBooleanLiteral() => _falseLiteral.Should().Contain("Is False");

    [Then("the age comparison should contain Minor")]
    [Trait("UAC", "60"), Fact]
    public void UAC60_HandlesLessThanComparisonForAge() => _ageComparison.Should().Contain("Minor");

    [Then("the non-null comparison should contain Has Value")]
    [Trait("UAC", "61"), Fact]
    public void UAC61_HandlesNotNullComparison() => _notNullValue.Should().Contain("Has Value");

    [Then("the non-zero integer render should contain Truthy")]
    [Trait("UAC", "62"), Fact]
    public void UAC62_TreatsNonZeroIntAsTruthy() => _nonZeroInt.Should().Contain("Truthy");

    [Then("the empty list render should contain No Items")]
    [Trait("UAC", "63"), Fact]
    public void UAC63_TreatsEmptyCollectionAsFalsy() => _emptyList.Should().Contain("No Items");

    [Then("the true flag render should contain True")]
    [Trait("UAC", "64"), Fact]
    public void UAC64_TreatsTrueBoolAsTruthy() => _trueFlag.Should().Contain("True");

    [Then("the false flag render should contain False")]
    [Trait("UAC", "65"), Fact]
    public void UAC65_TreatsFalseBoolAsFalsy() => _falseFlag.Should().Contain("False").And.NotContain("True\n");

    [Then("the object render should contain Has Object")]
    [Trait("UAC", "66"), Fact]
    public void UAC66_TreatsObjectAsTruthy() => _object.Should().Contain("Has Object");
}
