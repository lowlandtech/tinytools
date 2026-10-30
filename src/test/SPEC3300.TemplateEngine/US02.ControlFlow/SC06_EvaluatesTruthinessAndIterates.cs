using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "06")]
[UserStory("02", "Template engine evaluates truthiness and iterates collections")]
public sealed class SC06_EvaluatesTruthinessAndIterates : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _named = string.Empty;
    private string _emptyName = string.Empty;
    private string _nullName = string.Empty;
    private string _zeroCount = string.Empty;
    private string _filledList = string.Empty;
    private string _iteration = string.Empty;
    private string _emptyCollection = string.Empty;
    private string _nullCollection = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each truthiness and iteration template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("Name", "John");
        _named = Sut.Render("@if(Context.Name) {\nHas Name\n}", _context);
        _context.Set("Name", "");
        _emptyName = Sut.Render("@if(Context.Name) {\nHas Name\n}", _context);
        _context.Set("Name", null);
        _nullName = Sut.Render("@if(Context.Name) {\nHas Name\n}", _context);
        _context.Set("Count", 0);
        _zeroCount = Sut.Render("@if(Context.Count) {\nHas Count\n}", _context);
        _context.Set("Items", new List<string> { "a", "b" });
        _filledList = Sut.Render("@if(Context.Items) {\nHas Items\n}", _context);
        _context.Set("Items", new[] { "A", "B", "C" });
        _iteration = Sut.Render("@foreach(var item in Context.Items) {\n${item}\n}", _context);
        _context.Set("Items", new string[] { });
        _emptyCollection = Sut.Render("@foreach(var item in Context.Items) {\n${item}\n}", _context);
        _context.Set("Items", null);
        _nullCollection = Sut.Render("@foreach(var item in Context.Items) {\n${item}\n}", _context);
    }

    public SC06_EvaluatesTruthinessAndIterates() => ArrangeAndAct();

    [Then("the named context block should contain Has Name")]
    [Trait("UAC", "20"), Fact]
    public void UAC20_HandlesTruthyCheckForString() => _named.Should().Contain("Has Name");

    [Then("the empty name block should not contain Has Name")]
    [Trait("UAC", "21"), Fact]
    public void UAC21_HandlesTruthyCheckForEmptyString() => _emptyName.Should().NotContain("Has Name");

    [Then("the null name block should not contain Has Name")]
    [Trait("UAC", "22"), Fact]
    public void UAC22_HandlesTruthyCheckForNull() => _nullName.Should().NotContain("Has Name");

    [Then("the zero count block should not contain Has Count")]
    [Trait("UAC", "23"), Fact]
    public void UAC23_HandlesTruthyCheckForNumber() => _zeroCount.Should().NotContain("Has Count");

    [Then("the filled list block should contain Has Items")]
    [Trait("UAC", "24"), Fact]
    public void UAC24_HandlesTruthyCheckForCollection() => _filledList.Should().Contain("Has Items");

    [Then("the three item loop should contain every item")]
    [Trait("UAC", "25"), Fact]
    public void UAC25_IteratesOverCollection() => _iteration.Should().Contain("A").And.Contain("B").And.Contain("C");

    [Then("the empty collection loop should be empty")]
    [Trait("UAC", "26"), Fact]
    public void UAC26_HandlesEmptyCollection() => _emptyCollection.Should().BeEmpty();

    [Then("the null collection loop should be empty")]
    [Trait("UAC", "27"), Fact]
    public void UAC27_HandlesNullCollection() => _nullCollection.Should().BeEmpty();
}
