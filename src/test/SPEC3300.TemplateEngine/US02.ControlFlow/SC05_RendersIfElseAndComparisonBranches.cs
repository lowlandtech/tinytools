using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "05")]
[UserStory("02", "Template engine renders if, else and comparison branches")]
public sealed class SC05_RendersIfElseAndComparisonBranches : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;

    public SC05_RendersIfElseAndComparisonBranches(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("every if, else and comparison template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("IsActive", true);
        useCase.Cases["IfEnabled"] = Sut.Render("@if(Context.IsActive) {\nActive\n}", _context);
        _context.Set("IsActive", false);
        useCase.Cases["IfDisabled"] = Sut.Render("@if(Context.IsActive) {\nActive\n}", _context);
        useCase.Cases["ElseBranch"] = Sut.Render("@if(Context.IsActive) {\nActive\n} else {\nInactive\n}", _context);
        useCase.Cases["NegatedFlag"] = Sut.Render("@if(!Context.IsActive) {\nNot Active\n}", _context);
        _context.Set("Status", "pending");
        useCase.Cases["ElseIfBranch"] = Sut.Render("@if(Context.Status == \"active\") {\nActive\n} else if (Context.Status == \"pending\") {\nPending\n} else {\nUnknown\n}", _context);
        _context.Set("Count", 10);
        useCase.Cases["GreaterThan"] = Sut.Render("@if(Context.Count > 5) {\nGreater\n}", _context);
        _context.Set("Count", 5);
        useCase.Cases["GreaterOrEqual"] = Sut.Render("@if(Context.Count >= 5) {\nGreater or Equal\n}", _context);
        useCase.Cases["LessOrEqual"] = Sut.Render("@if(Context.Count <= 5) {\nLess or Equal\n}", _context);
        _context.Set("Count", 3);
        useCase.Cases["LessThan"] = Sut.Render("@if(Context.Count < 5) {\nLess\n}", _context);
        _context.Set("Status", "active");
        useCase.Cases["Equality"] = Sut.Render("@if(Context.Status == \"active\") {\nEqual\n}", _context);
        _context.Set("Status", "inactive");
        useCase.Cases["Inequality"] = Sut.Render("@if(Context.Status != \"active\") {\nNot Equal\n}", _context);
    }

    [Then("the active if block should contain Active")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_RendersIfBlockWhenConditionIsTrue() => ((string)useCase.Cases["IfEnabled"]!).Should().Contain("Active");

    [Then("the inactive if block should not contain Active")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_OmitsIfBlockWhenConditionIsFalse() => ((string)useCase.Cases["IfDisabled"]!).Should().NotContain("Active");

    [Then("the else block should contain Inactive")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_RendersElseBlockWhenConditionIsFalse() => ((string)useCase.Cases["ElseBranch"]!).Should().Contain("Inactive").And.NotContain("Active");

    [Then("the else-if chain should contain Pending")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_RendersElseIfBlockWhenConditionMatches() => ((string)useCase.Cases["ElseIfBranch"]!).Should().Contain("Pending").And.NotContain("Active").And.NotContain("Unknown");

    [Then("the negated flag block should contain Not Active")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesNegationOperator() => ((string)useCase.Cases["NegatedFlag"]!).Should().Contain("Not Active");

    [Then("the greater comparison block should contain Greater")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_HandlesGreaterThanComparison() => ((string)useCase.Cases["GreaterThan"]!).Should().Contain("Greater");

    [Then("the greater-or-equal comparison block should contain Greater or Equal")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_HandlesGreaterThanOrEqualComparison() => ((string)useCase.Cases["GreaterOrEqual"]!).Should().Contain("Greater or Equal");

    [Then("the less comparison block should contain Less")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_HandlesLessThanComparison() => ((string)useCase.Cases["LessThan"]!).Should().Contain("Less");

    [Then("the less-or-equal comparison block should contain Less or Equal")]
    [Trait("UAC", "17"), Fact]
    public void UAC17_HandlesLessThanOrEqualComparison() => ((string)useCase.Cases["LessOrEqual"]!).Should().Contain("Less or Equal");

    [Then("the equality comparison block should contain Equal")]
    [Trait("UAC", "18"), Fact]
    public void UAC18_HandlesEqualityComparison() => ((string)useCase.Cases["Equality"]!).Should().Contain("Equal");

    [Then("the inequality comparison block should contain Not Equal")]
    [Trait("UAC", "19"), Fact]
    public void UAC19_HandlesInequalityComparison() => ((string)useCase.Cases["Inequality"]!).Should().Contain("Not Equal");
}
