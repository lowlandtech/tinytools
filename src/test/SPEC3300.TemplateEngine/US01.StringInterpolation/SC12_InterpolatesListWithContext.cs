using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "12")]
[UserStory("01", "String interpolation handles list templates with context")]
public sealed class SC12_InterpolatesListWithContext : TinyToolsScenario<List<string>>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private ExecutionContext _context = null!;

    public SC12_InterpolatesListWithContext(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["All"] = Sut.Interpolate(_context, engine: null);
        useCase.Cases["Custom"] = new List<string> { "Hello ${Context.Name}", "Bye ${Context.Name}" }.Interpolate(ContextWith("Name", "Jane"), new TinyTemplateEngine());
        useCase.Cases["Empty"] = new List<string>().Interpolate(_context, engine: null);
        useCase.Cases["Single"] = new List<string> { "Hello ${Context.Name}" }.Interpolate(ContextWith("Name", "John"), engine: null);
        useCase.Cases["Model"] = new List<string> { "${Context.Model.FirstName}", "${Context.Model.LastName}" }.Interpolate(ContextWith("Model", new Person { FirstName = "John", LastName = "Doe" }), engine: null);
        useCase.Cases["NullTemplates"] = (Action)(() => ((List<string>)null!).Interpolate(_context, engine: null));
        useCase.Cases["NullContext"] = (Action)(() => new List<string> { "Hello" }.Interpolate((ExecutionContext)null!, engine: null));
    }

    private static ExecutionContext ContextWith(string key, object value)
    {
        var context = new ExecutionContext();
        context.Set(key, value);
        return context;
    }

    [Then("the interpolated list should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesAllTemplates() => ((List<string>)useCase.Cases["All"]!).Should().Equal("Hello John", "Welcome to Seattle", "Your age is 30");

    [Then("the custom engine list should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_InterpolatesWithCustomEngine() => ((List<string>)useCase.Cases["Custom"]!).Should().Equal("Hello Jane", "Bye Jane");

    [Then("the empty list should be empty")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HandlesEmptyList() => ((List<string>)useCase.Cases["Empty"]!).Should().BeEmpty();

    [Then("the single item list should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HandlesSingleItemList() => ((List<string>)useCase.Cases["Single"]!).Should().Equal("Hello John");

    [Then("the null templates should fail with ArgumentNullException")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ThrowsOnNullTemplates() => ((Action)useCase.Cases["NullTemplates"]!).Should().Throw<ArgumentNullException>();

    [Then("the null context should fail with ArgumentNullException")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ThrowsOnNullContext() => ((Action)useCase.Cases["NullContext"]!).Should().Throw<ArgumentNullException>();

    [Then("the model list should be exact")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_InterpolatesModelFromContext() => ((List<string>)useCase.Cases["Model"]!).Should().Equal("John", "Doe");
}
