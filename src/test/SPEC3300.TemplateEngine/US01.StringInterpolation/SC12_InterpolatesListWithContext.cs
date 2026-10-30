using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "12")]
[UserStory("01", "String interpolation handles list templates with context")]
public sealed class SC12_InterpolatesListWithContext : TinyToolsScenario<List<string>>
{
    private ExecutionContext _context = null!;
    private List<string> _all = [];
    private List<string> _custom = [];
    private List<string> _empty = [];
    private List<string> _single = [];
    private List<string> _model = [];
    private Action? _nullTemplates;
    private Action? _nullContext;

    protected override List<string> For() =>
    [
        "Hello ${Context.Name}",
        "Welcome to ${Context.City}",
        "Your age is ${Context.Age}",
    ];

    [Given("an execution context holding a name, a city and an age")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Name", "John");
        _context.Set("City", "Seattle");
        _context.Set("Age", 30);
    }

    [When("each list template set is interpolated once")]
    protected override void When()
    {
        base.When();

        _all = Sut.Interpolate(_context, engine: null);
        _custom = new List<string> { "Hello ${Context.Name}", "Bye ${Context.Name}" }.Interpolate(ContextWith("Name", "Jane"), new TinyTemplateEngine());
        _empty = new List<string>().Interpolate(_context, engine: null);
        _single = new List<string> { "Hello ${Context.Name}" }.Interpolate(ContextWith("Name", "John"), engine: null);
        _model = new List<string> { "${Context.Model.FirstName}", "${Context.Model.LastName}" }.Interpolate(ContextWith("Model", new Person { FirstName = "John", LastName = "Doe" }), engine: null);
        _nullTemplates = () => ((List<string>)null!).Interpolate(_context, engine: null);
        _nullContext = () => new List<string> { "Hello" }.Interpolate((ExecutionContext)null!, engine: null);
    }

    private static ExecutionContext ContextWith(string key, object value)
    {
        var context = new ExecutionContext();
        context.Set(key, value);
        return context;
    }

    public SC12_InterpolatesListWithContext() => ArrangeAndAct();

    [Then("the interpolated list should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesAllTemplates() => _all.Should().Equal("Hello John", "Welcome to Seattle", "Your age is 30");

    [Then("the custom engine list should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_InterpolatesWithCustomEngine() => _custom.Should().Equal("Hello Jane", "Bye Jane");

    [Then("the empty list should be empty")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HandlesEmptyList() => _empty.Should().BeEmpty();

    [Then("the single item list should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HandlesSingleItemList() => _single.Should().Equal("Hello John");

    [Then("the null templates should fail with ArgumentNullException")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ThrowsOnNullTemplates() => _nullTemplates.Should().Throw<ArgumentNullException>();

    [Then("the null context should fail with ArgumentNullException")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ThrowsOnNullContext() => _nullContext.Should().Throw<ArgumentNullException>();

    [Then("the model list should be exact")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_InterpolatesModelFromContext() => _model.Should().Equal("John", "Doe");
}
