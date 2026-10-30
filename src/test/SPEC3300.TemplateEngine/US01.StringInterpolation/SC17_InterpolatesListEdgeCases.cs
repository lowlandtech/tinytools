using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "17")]
[UserStory("01", "String interpolation handles list templates with context")]
public sealed class SC17_InterpolatesListEdgeCases : TinyToolsScenario<List<string>>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private ExecutionContext _context = null!;

    public SC17_InterpolatesListEdgeCases(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["NoVariables"] = new List<string> { "Hello world!", "Goodbye!" }.Interpolate(_context, engine: null);
        useCase.Cases["Mixed"] = new List<string> { "Static text", "Hello ${Context.Name}", "Another static", "${Context.City} is great" }.Interpolate(_context, engine: null);
        useCase.Cases["Conditional"] = new List<string> { "@if (Context.IsActive) {\nActive\n}" }.Interpolate(ContextWith("IsActive", true), engine: null);
        useCase.Cases["Ordered"] = new List<string> { "${Context.A}", "${Context.B}", "${Context.C}", "${Context.D}" }.Interpolate(ContextWith("A", "1", "B", "2", "C", "3", "D", "4"), engine: null);
        useCase.Cases["Duplicates"] = new List<string> { "Hello ${Context.Name}", "Hello ${Context.Name}", "Hello ${Context.Name}" }.Interpolate(_context, engine: null);
        useCase.Cases["Nested"] = new List<string> { "${Context.User.Name}", "${Context.User.Address.City}" }.Interpolate(ContextWith("User", new { Name = "John", Address = new { City = "Seattle" } }), engine: null);
        useCase.Cases["Coalesced"] = new List<string> { "${Context.Existing ?? \"default1\"}", "${Context.Missing ?? \"default2\"}" }.Interpolate(ContextWith("Existing", "value"), engine: null);
    }

    private static ExecutionContext ContextWith(params object[] pairs)
    {
        var context = new ExecutionContext();
        for (var index = 0; index < pairs.Length; index += 2)
            context.Set((string)pairs[index], pairs[index + 1]);
        return context;
    }

    [Then("the static list should be exact")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_HandlesTemplatesWithNoVariables() => ((List<string>)useCase.Cases["NoVariables"]!).Should().Equal("Hello world!", "Goodbye!");

    [Then("the mixed list should be exact")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_HandlesMixedTemplates() => ((List<string>)useCase.Cases["Mixed"]!).Should().Equal("Static text", "Hello John", "Another static", "Seattle is great");

    [Then("the conditional list should contain Active")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_SupportsIfControlFlowInList() => ((List<string>)useCase.Cases["Conditional"]!)[0].Should().Contain("Active");

    [Then("the ordered list should contain 1")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_PreservesListOrder() => ((List<string>)useCase.Cases["Ordered"]!).Should().ContainInOrder("1", "2", "3", "4");

    [Then("the duplicate list should be exact")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_HandlesDuplicateTemplates() => ((List<string>)useCase.Cases["Duplicates"]!).Should().HaveCount(3).And.AllBe("Hello John");

    [Then("the nested list should be exact")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesNestedProperties() => ((List<string>)useCase.Cases["Nested"]!).Should().Equal("John", "Seattle");

    [Then("the coalesced list should be exact")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_SupportsNullCoalescingInList() => ((List<string>)useCase.Cases["Coalesced"]!).Should().Equal("value", "default2");
}
