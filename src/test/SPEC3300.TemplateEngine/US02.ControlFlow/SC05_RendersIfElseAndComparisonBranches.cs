using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "05")]
[UserStory("02", "Template engine renders if, else and comparison branches")]
public sealed class SC05_RendersIfElseAndComparisonBranches : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _ifEnabled = string.Empty;
    private string _ifDisabled = string.Empty;
    private string _elseBranch = string.Empty;
    private string _elseIfBranch = string.Empty;
    private string _negatedFlag = string.Empty;
    private string _greaterThan = string.Empty;
    private string _greaterOrEqual = string.Empty;
    private string _lessThan = string.Empty;
    private string _lessOrEqual = string.Empty;
    private string _equality = string.Empty;
    private string _inequality = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("every if, else and comparison template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("IsActive", true);
        _ifEnabled = Sut.Render("@if(Context.IsActive) {\nActive\n}", _context);
        _context.Set("IsActive", false);
        _ifDisabled = Sut.Render("@if(Context.IsActive) {\nActive\n}", _context);
        _elseBranch = Sut.Render("@if(Context.IsActive) {\nActive\n} else {\nInactive\n}", _context);
        _negatedFlag = Sut.Render("@if(!Context.IsActive) {\nNot Active\n}", _context);
        _context.Set("Status", "pending");
        _elseIfBranch = Sut.Render("@if(Context.Status == \"active\") {\nActive\n} else if (Context.Status == \"pending\") {\nPending\n} else {\nUnknown\n}", _context);
        _context.Set("Count", 10);
        _greaterThan = Sut.Render("@if(Context.Count > 5) {\nGreater\n}", _context);
        _context.Set("Count", 5);
        _greaterOrEqual = Sut.Render("@if(Context.Count >= 5) {\nGreater or Equal\n}", _context);
        _lessOrEqual = Sut.Render("@if(Context.Count <= 5) {\nLess or Equal\n}", _context);
        _context.Set("Count", 3);
        _lessThan = Sut.Render("@if(Context.Count < 5) {\nLess\n}", _context);
        _context.Set("Status", "active");
        _equality = Sut.Render("@if(Context.Status == \"active\") {\nEqual\n}", _context);
        _context.Set("Status", "inactive");
        _inequality = Sut.Render("@if(Context.Status != \"active\") {\nNot Equal\n}", _context);
    }

    public SC05_RendersIfElseAndComparisonBranches() => ArrangeAndAct();

    [Then("the active if block should contain Active")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_RendersIfBlockWhenConditionIsTrue() => _ifEnabled.Should().Contain("Active");

    [Then("the inactive if block should not contain Active")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_OmitsIfBlockWhenConditionIsFalse() => _ifDisabled.Should().NotContain("Active");

    [Then("the else block should contain Inactive")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_RendersElseBlockWhenConditionIsFalse() => _elseBranch.Should().Contain("Inactive").And.NotContain("Active");

    [Then("the else-if chain should contain Pending")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_RendersElseIfBlockWhenConditionMatches() => _elseIfBranch.Should().Contain("Pending").And.NotContain("Active").And.NotContain("Unknown");

    [Then("the negated flag block should contain Not Active")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesNegationOperator() => _negatedFlag.Should().Contain("Not Active");

    [Then("the greater comparison block should contain Greater")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_HandlesGreaterThanComparison() => _greaterThan.Should().Contain("Greater");

    [Then("the greater-or-equal comparison block should contain Greater or Equal")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_HandlesGreaterThanOrEqualComparison() => _greaterOrEqual.Should().Contain("Greater or Equal");

    [Then("the less comparison block should contain Less")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_HandlesLessThanComparison() => _lessThan.Should().Contain("Less");

    [Then("the less-or-equal comparison block should contain Less or Equal")]
    [Trait("UAC", "17"), Fact]
    public void UAC17_HandlesLessThanOrEqualComparison() => _lessOrEqual.Should().Contain("Less or Equal");

    [Then("the equality comparison block should contain Equal")]
    [Trait("UAC", "18"), Fact]
    public void UAC18_HandlesEqualityComparison() => _equality.Should().Contain("Equal");

    [Then("the inequality comparison block should contain Not Equal")]
    [Trait("UAC", "19"), Fact]
    public void UAC19_HandlesInequalityComparison() => _inequality.Should().Contain("Not Equal");
}
