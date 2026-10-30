using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "10")]
[UserStory("01", "String interpolation uses execution context")]
public sealed class SC10_InterpolatesWithContext : TinyToolsScenario<string>
{
    private ExecutionContext _context = null!;
    private string _name = string.Empty;
    private string _custom = string.Empty;
    private string _model = string.Empty;
    private string _nested = string.Empty;
    private string _conditional = string.Empty;
    private string _loop = string.Empty;
    private Action? _nullTemplate;
    private Action? _emptyTemplate;

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

        _name = Sut.Interpolate(_context);
        _custom = "Hello ${Context.Name}".Interpolate(ContextWith("Name", "Jane"), new TinyTemplateEngine());
        _model = "Hello ${Context.Model.FirstName}".Interpolate(new ExecutionContext { Model = new Person { FirstName = "John" } });
        _nested = "City: ${Context.Address.City}".Interpolate(ContextWith("Address", new { City = "Seattle", State = "WA" }));
        _conditional = "@if (Context.IsActive) {\nActive User\n}".Interpolate(ContextWith("IsActive", true));
        _loop = "@foreach (var item in Context.Items) {\n- ${item}\n}".Interpolate(ContextWith("Items", new[] { "Apple", "Banana", "Cherry" }));
        _nullTemplate = () => ((string)null!).Interpolate(_context);
        _emptyTemplate = () => "".Interpolate(_context);
    }

    private static ExecutionContext ContextWith(string key, object value)
    {
        var context = new ExecutionContext();
        context.Set(key, value);
        return context;
    }

    public SC10_InterpolatesWithContext() => ArrangeAndAct();

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesContextValues() => _name.Should().Be("Hello world, I'm John Smith");

    [Then("the custom engine output should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_InterpolatesWithCustomEngine() => _custom.Should().Be("Hello Jane");

    [Then("the model output should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_InterpolatesModelFromContext() => _model.Should().Be("Hello John");

    [Then("the nested output should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_InterpolatesNestedContextValues() => _nested.Should().Be("City: Seattle");

    [Then("the conditional output should contain Active User")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_SupportsIfControlFlow() => _conditional.Should().Contain("Active User");

    [Then("the loop output should contain Apple")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SupportsForeachControlFlow() => _loop.Should().Contain("Apple").And.Contain("Banana").And.Contain("Cherry");

    [Then("the interpolation should fail with ArgumentException")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ThrowsOnNullTemplate() => _nullTemplate.Should().Throw<ArgumentException>();

    [Then("the empty template should fail with ArgumentException")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_ThrowsOnEmptyTemplate() => _emptyTemplate.Should().Throw<ArgumentException>();
}
