using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "11")]
[UserStory("02", "Template engine compares typed values and evaluates truthiness")]
public sealed class SC11_ComparesTypedValuesAndTruthiness : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;

    public SC11_ComparesTypedValuesAndTruthiness(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each typed comparison and truthiness template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("Value", 3.14);
        useCase.Cases["DoubleValue"] = Sut.Render("@if (Context.Value > 3) {\nGreater\n}", _context);
        _context.Set("Value", 99.99m);
        useCase.Cases["DecimalValue"] = Sut.Render("@if (Context.Value >= 100) {\nExpensive\n} else {\nAffordable\n}", _context);
        _context.Set("Flag", true);
        useCase.Cases["BooleanLiteral"] = Sut.Render("@if (Context.Flag == true) {\nIs True\n}", _context);
        useCase.Cases["TrueFlag"] = Sut.Render("@if (Context.Flag) {\nTrue\n}", _context);
        _context.Set("Flag", false);
        useCase.Cases["FalseLiteral"] = Sut.Render("@if (Context.Flag == false) {\nIs False\n}", _context);
        useCase.Cases["FalseFlag"] = Sut.Render("@if (Context.Flag) {\nTrue\n} else {\nFalse\n}", _context);
        _context.Set("Age", 17);
        useCase.Cases["AgeComparison"] = Sut.Render("@if (Context.Age < 18) {\nMinor\n}", _context);
        _context.Set("Value", "something");
        useCase.Cases["NotNullValue"] = Sut.Render("@if (Context.Value != null) {\nHas Value\n}", _context);
        _context.Set("Value", -1);
        useCase.Cases["NonZeroInt"] = Sut.Render("@if (Context.Value) {\nTruthy\n}", _context);
        _context.Set("Items", new List<string>());
        useCase.Cases["EmptyList"] = Sut.Render("@if (Context.Items) {\nHas Items\n} else {\nNo Items\n}", _context);
        _context.Set("Obj", new { Name = "Test" });
        useCase.Cases["Object"] = Sut.Render("@if (Context.Obj) {\nHas Object\n}", _context);
    }

    [Then("the double value comparison should contain Greater")]
    [Trait("UAC", "56"), Fact]
    public void UAC56_HandlesNumericComparisonWithDoubles() => ((string)useCase.Cases["DoubleValue"]!).Should().Contain("Greater");

    [Then("the decimal value comparison should contain Affordable")]
    [Trait("UAC", "57"), Fact]
    public void UAC57_HandlesNumericComparisonWithDecimals() => ((string)useCase.Cases["DecimalValue"]!).Should().Contain("Affordable");

    [Then("the true literal comparison should contain Is True")]
    [Trait("UAC", "58"), Fact]
    public void UAC58_HandlesComparisonWithBooleanLiteral() => ((string)useCase.Cases["BooleanLiteral"]!).Should().Contain("Is True");

    [Then("the false literal comparison should contain Is False")]
    [Trait("UAC", "59"), Fact]
    public void UAC59_HandlesComparisonWithFalseBooleanLiteral() => ((string)useCase.Cases["FalseLiteral"]!).Should().Contain("Is False");

    [Then("the age comparison should contain Minor")]
    [Trait("UAC", "60"), Fact]
    public void UAC60_HandlesLessThanComparisonForAge() => ((string)useCase.Cases["AgeComparison"]!).Should().Contain("Minor");

    [Then("the non-null comparison should contain Has Value")]
    [Trait("UAC", "61"), Fact]
    public void UAC61_HandlesNotNullComparison() => ((string)useCase.Cases["NotNullValue"]!).Should().Contain("Has Value");

    [Then("the non-zero integer render should contain Truthy")]
    [Trait("UAC", "62"), Fact]
    public void UAC62_TreatsNonZeroIntAsTruthy() => ((string)useCase.Cases["NonZeroInt"]!).Should().Contain("Truthy");

    [Then("the empty list render should contain No Items")]
    [Trait("UAC", "63"), Fact]
    public void UAC63_TreatsEmptyCollectionAsFalsy() => ((string)useCase.Cases["EmptyList"]!).Should().Contain("No Items");

    [Then("the true flag render should contain True")]
    [Trait("UAC", "64"), Fact]
    public void UAC64_TreatsTrueBoolAsTruthy() => ((string)useCase.Cases["TrueFlag"]!).Should().Contain("True");

    [Then("the false flag render should contain False")]
    [Trait("UAC", "65"), Fact]
    public void UAC65_TreatsFalseBoolAsFalsy() => ((string)useCase.Cases["FalseFlag"]!).Should().Contain("False").And.NotContain("True\n");

    [Then("the object render should contain Has Object")]
    [Trait("UAC", "66"), Fact]
    public void UAC66_TreatsObjectAsTruthy() => ((string)useCase.Cases["Object"]!).Should().Contain("Has Object");
}
