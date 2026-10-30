using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "06")]
[UserStory("02", "Template engine evaluates truthiness and iterates collections")]
public sealed class SC06_EvaluatesTruthinessAndIterates : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;

    public SC06_EvaluatesTruthinessAndIterates(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each truthiness and iteration template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("Name", "John");
        useCase.Cases["Named"] = Sut.Render("@if(Context.Name) {\nHas Name\n}", _context);
        _context.Set("Name", "");
        useCase.Cases["EmptyName"] = Sut.Render("@if(Context.Name) {\nHas Name\n}", _context);
        _context.Set("Name", null);
        useCase.Cases["NullName"] = Sut.Render("@if(Context.Name) {\nHas Name\n}", _context);
        _context.Set("Count", 0);
        useCase.Cases["ZeroCount"] = Sut.Render("@if(Context.Count) {\nHas Count\n}", _context);
        _context.Set("Items", new List<string> { "a", "b" });
        useCase.Cases["FilledList"] = Sut.Render("@if(Context.Items) {\nHas Items\n}", _context);
        _context.Set("Items", new[] { "A", "B", "C" });
        useCase.Cases["Iteration"] = Sut.Render("@foreach(var item in Context.Items) {\n${item}\n}", _context);
        _context.Set("Items", new string[] { });
        useCase.Cases["EmptyCollection"] = Sut.Render("@foreach(var item in Context.Items) {\n${item}\n}", _context);
        _context.Set("Items", null);
        useCase.Cases["NullCollection"] = Sut.Render("@foreach(var item in Context.Items) {\n${item}\n}", _context);
    }

    [Then("the named context block should contain Has Name")]
    [Trait("UAC", "20"), Fact]
    public void UAC20_HandlesTruthyCheckForString() => ((string)useCase.Cases["Named"]!).Should().Contain("Has Name");

    [Then("the empty name block should not contain Has Name")]
    [Trait("UAC", "21"), Fact]
    public void UAC21_HandlesTruthyCheckForEmptyString() => ((string)useCase.Cases["EmptyName"]!).Should().NotContain("Has Name");

    [Then("the null name block should not contain Has Name")]
    [Trait("UAC", "22"), Fact]
    public void UAC22_HandlesTruthyCheckForNull() => ((string)useCase.Cases["NullName"]!).Should().NotContain("Has Name");

    [Then("the zero count block should not contain Has Count")]
    [Trait("UAC", "23"), Fact]
    public void UAC23_HandlesTruthyCheckForNumber() => ((string)useCase.Cases["ZeroCount"]!).Should().NotContain("Has Count");

    [Then("the filled list block should contain Has Items")]
    [Trait("UAC", "24"), Fact]
    public void UAC24_HandlesTruthyCheckForCollection() => ((string)useCase.Cases["FilledList"]!).Should().Contain("Has Items");

    [Then("the three item loop should contain every item")]
    [Trait("UAC", "25"), Fact]
    public void UAC25_IteratesOverCollection() => ((string)useCase.Cases["Iteration"]!).Should().Contain("A").And.Contain("B").And.Contain("C");

    [Then("the empty collection loop should be empty")]
    [Trait("UAC", "26"), Fact]
    public void UAC26_HandlesEmptyCollection() => ((string)useCase.Cases["EmptyCollection"]!).Should().BeEmpty();

    [Then("the null collection loop should be empty")]
    [Trait("UAC", "27"), Fact]
    public void UAC27_HandlesNullCollection() => ((string)useCase.Cases["NullCollection"]!).Should().BeEmpty();
}
