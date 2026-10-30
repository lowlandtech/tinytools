using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "10")]
[UserStory("01", "String interpolation uses execution context")]
public sealed class SC10_InterpolatesWithContext : TinyToolsScenario<string>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private ExecutionContext _context = null!;

    public SC10_InterpolatesWithContext(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override string For() => "Hello world, I'm ${Context.FirstName} ${Context.LastName}";

    [Given("an execution context holding a first and last name")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("FirstName", "John");
        _context.Set("LastName", "Smith");
    }

    [When("each context template is interpolated once")]
    protected override void When()
    {
        base.When();

        useCase.Cases["ContextValues"] = Sut.Interpolate(_context);
        useCase.Cases["CustomEngine"] = "Hello ${Context.Name}".Interpolate(ContextWith("Name", "Jane"), new TinyTemplateEngine());
        useCase.Cases["ContextModel"] = "Hello ${Context.Model.FirstName}".Interpolate(new ExecutionContext { Model = new Person { FirstName = "John" } });
        useCase.Cases["NestedContext"] = "City: ${Context.Address.City}".Interpolate(ContextWith("Address", new { City = "Seattle", State = "WA" }));
        useCase.Cases["Conditional"] = "@if (Context.IsActive) {\nActive User\n}".Interpolate(ContextWith("IsActive", true));
        useCase.Cases["Loop"] = "@foreach (var item in Context.Items) {\n- ${item}\n}".Interpolate(ContextWith("Items", new[] { "Apple", "Banana", "Cherry" }));
        useCase.Cases["NullTemplate"] = (Action)(() => ((string)null!).Interpolate(_context));
        useCase.Cases["EmptyTemplate"] = (Action)(() => "".Interpolate(_context));
    }

    private static ExecutionContext ContextWith(string key, object value)
    {
        var context = new ExecutionContext();
        context.Set(key, value);
        return context;
    }

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesContextValues() => useCase.Cases["ContextValues"].Should().Be("Hello world, I'm John Smith");

    [Then("the custom engine output should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_InterpolatesWithCustomEngine() => useCase.Cases["CustomEngine"].Should().Be("Hello Jane");

    [Then("the model output should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_InterpolatesModelFromContext() => useCase.Cases["ContextModel"].Should().Be("Hello John");

    [Then("the nested output should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_InterpolatesNestedContextValues() => useCase.Cases["NestedContext"].Should().Be("City: Seattle");

    [Then("the conditional output should contain Active User")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_SupportsIfControlFlow() => ((string)useCase.Cases["Conditional"]!).Should().Contain("Active User");

    [Then("the loop output should contain Apple")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SupportsForeachControlFlow() => ((string)useCase.Cases["Loop"]!).Should().Contain("Apple").And.Contain("Banana").And.Contain("Cherry");

    [Then("the interpolation should fail with ArgumentException")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ThrowsOnNullTemplate() => ((Action)useCase.Cases["NullTemplate"]!).Should().Throw<ArgumentException>();

    [Then("the empty template should fail with ArgumentException")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_ThrowsOnEmptyTemplate() => ((Action)useCase.Cases["EmptyTemplate"]!).Should().Throw<ArgumentException>();
}
