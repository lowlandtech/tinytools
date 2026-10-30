using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "15")]
[UserStory("01", "String interpolation handles context values and edge cases")]
public sealed class SC15_InterpolatesContextEdgeCases : TinyToolsScenario<string>
{
    private ExecutionContext _context = null!;
    private Action? _nullContext;
    private string _multiple = string.Empty;
    private string _integer = string.Empty;
    private string _literal = string.Empty;
    private string _noVariables = string.Empty;
    private string _coalesced = string.Empty;
    private string _child = string.Empty;

    protected override string For() => "Hello ${Context.Name}";

    [Given("an execution context holding a name")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Name", "John");
    }

    [When("each context template is interpolated once")]
    protected override void When()
    {
        base.When();

        _nullContext = () => Sut.Interpolate((ExecutionContext)null!);
        _multiple = "${Context.A} + ${Context.B} = ${Context.C}".Interpolate(ContextWith("A", "1", "B", "2", "C", "3"));
        _integer = "Count: ${Context.Count}".Interpolate(ContextWith("Count", 42));
        _literal = "Hello world! ${Context.Name} says hi.".Interpolate(_context);
        _noVariables = "Hello world!".Interpolate(_context);
        _coalesced = "${Context.Missing ?? \"default\"}".Interpolate(new ExecutionContext());

        var parent = ContextWith("ParentValue", "Parent");
        var kid = parent.CreateChild();
        kid.Set("ChildValue", "Child");
        _child = "${Context.ParentValue} - ${Context.ChildValue}".Interpolate(kid);
    }

    private static ExecutionContext ContextWith(params object[] pairs)
    {
        var context = new ExecutionContext();
        for (var index = 0; index < pairs.Length; index += 2)
            context.Set((string)pairs[index], pairs[index + 1]);
        return context;
    }

    public SC15_InterpolatesContextEdgeCases() => ArrangeAndAct();

    [Then("the interpolation should fail with ArgumentNullException")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_ThrowsOnNullContext() => _nullContext.Should().Throw<ArgumentNullException>();

    [Then("the multiple variable text should be exact")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_HandlesMultipleVariables() => _multiple.Should().Be("1 + 2 = 3");

    [Then("the integer text should be exact")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_HandlesIntegerValues() => _integer.Should().Be("Count: 42");

    [Then("the literal text should be exact")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_PreservesNonInterpolatedText() => _literal.Should().Be("Hello world! John says hi.");

    [Then("the template without variables should be exact")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesTemplateWithNoVariables() => _noVariables.Should().Be("Hello world!");

    [Then("the coalesced text should be exact")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_SupportsNullCoalescing() => _coalesced.Should().Be("default");

    [Then("the child context text should be exact")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_SupportsChildContext() => _child.Should().Be("Parent - Child");
}
