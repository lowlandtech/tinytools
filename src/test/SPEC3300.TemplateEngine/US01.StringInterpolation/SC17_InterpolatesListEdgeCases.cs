using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "17")]
[UserStory("01", "String interpolation handles list templates with context")]
public sealed class SC17_InterpolatesListEdgeCases : TinyToolsScenario<List<string>>
{
    private ExecutionContext _context = null!;
    private List<string> _noVariables = [];
    private List<string> _mixed = [];
    private List<string> _conditional = [];
    private List<string> _ordered = [];
    private List<string> _duplicates = [];
    private List<string> _nested = [];
    private List<string> _coalesced = [];

    protected override List<string> For() => ["Hello ${Context.Name}"];

    [Given("an execution context holding a name and a city")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Name", "John");
        _context.Set("City", "Seattle");
    }

    [When("each list template set is interpolated once")]
    protected override void When()
    {
        base.When();

        _noVariables = new List<string> { "Hello world!", "Goodbye!" }.Interpolate(_context, engine: null);
        _mixed = new List<string> { "Static text", "Hello ${Context.Name}", "Another static", "${Context.City} is great" }.Interpolate(_context, engine: null);
        _conditional = new List<string> { "@if (Context.IsActive) {\nActive\n}" }.Interpolate(ContextWith("IsActive", true), engine: null);
        _ordered = new List<string> { "${Context.A}", "${Context.B}", "${Context.C}", "${Context.D}" }.Interpolate(ContextWith("A", "1", "B", "2", "C", "3", "D", "4"), engine: null);
        _duplicates = new List<string> { "Hello ${Context.Name}", "Hello ${Context.Name}", "Hello ${Context.Name}" }.Interpolate(_context, engine: null);
        _nested = new List<string> { "${Context.User.Name}", "${Context.User.Address.City}" }.Interpolate(ContextWith("User", new { Name = "John", Address = new { City = "Seattle" } }), engine: null);
        _coalesced = new List<string> { "${Context.Existing ?? \"default1\"}", "${Context.Missing ?? \"default2\"}" }.Interpolate(ContextWith("Existing", "value"), engine: null);
    }

    private static ExecutionContext ContextWith(params object[] pairs)
    {
        var context = new ExecutionContext();
        for (var index = 0; index < pairs.Length; index += 2)
            context.Set((string)pairs[index], pairs[index + 1]);
        return context;
    }

    public SC17_InterpolatesListEdgeCases() => ArrangeAndAct();

    [Then("the static list should be exact")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_HandlesTemplatesWithNoVariables() => _noVariables.Should().Equal("Hello world!", "Goodbye!");

    [Then("the mixed list should be exact")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_HandlesMixedTemplates() => _mixed.Should().Equal("Static text", "Hello John", "Another static", "Seattle is great");

    [Then("the conditional list should contain Active")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_SupportsIfControlFlowInList() => _conditional[0].Should().Contain("Active");

    [Then("the ordered list should contain 1")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_PreservesListOrder() => _ordered.Should().ContainInOrder("1", "2", "3", "4");

    [Then("the duplicate list should be exact")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_HandlesDuplicateTemplates() => _duplicates.Should().HaveCount(3).And.AllBe("Hello John");

    [Then("the nested list should be exact")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesNestedProperties() => _nested.Should().Equal("John", "Seattle");

    [Then("the coalesced list should be exact")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_SupportsNullCoalescingInList() => _coalesced.Should().Equal("value", "default2");
}
