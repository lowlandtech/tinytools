using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "15")]
[UserStory("01", "String interpolation handles context values and edge cases")]
public sealed class SC15_InterpolatesContextEdgeCases : TinyToolsScenario<string>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private ExecutionContext _context = null!;

    public SC15_InterpolatesContextEdgeCases(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["NullContext"] = (Action)(() => Sut.Interpolate((ExecutionContext)null!));
        useCase.Cases["MultipleVariables"] = "${Context.A} + ${Context.B} = ${Context.C}".Interpolate(ContextWith("A", "1", "B", "2", "C", "3"));
        useCase.Cases["Integer"] = "Count: ${Context.Count}".Interpolate(ContextWith("Count", 42));
        useCase.Cases["Literal"] = "Hello world! ${Context.Name} says hi.".Interpolate(_context);
        useCase.Cases["NoVariables"] = "Hello world!".Interpolate(_context);
        useCase.Cases["Coalesced"] = "${Context.Missing ?? \"default\"}".Interpolate(new ExecutionContext());

        var parent = ContextWith("ParentValue", "Parent");
        var kid = parent.CreateChild();
        kid.Set("ChildValue", "Child");
        useCase.Cases["Child"] = "${Context.ParentValue} - ${Context.ChildValue}".Interpolate(kid);
    }

    private static ExecutionContext ContextWith(params object[] pairs)
    {
        var context = new ExecutionContext();
        for (var index = 0; index < pairs.Length; index += 2)
            context.Set((string)pairs[index], pairs[index + 1]);
        return context;
    }

    [Then("the interpolation should fail with ArgumentNullException")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_ThrowsOnNullContext() => ((Action)useCase.Cases["NullContext"]!).Should().Throw<ArgumentNullException>();

    [Then("the multiple variable text should be exact")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_HandlesMultipleVariables() => useCase.Cases["MultipleVariables"].Should().Be("1 + 2 = 3");

    [Then("the integer text should be exact")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_HandlesIntegerValues() => useCase.Cases["Integer"].Should().Be("Count: 42");

    [Then("the literal text should be exact")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_PreservesNonInterpolatedText() => useCase.Cases["Literal"].Should().Be("Hello world! John says hi.");

    [Then("the template without variables should be exact")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesTemplateWithNoVariables() => useCase.Cases["NoVariables"].Should().Be("Hello world!");

    [Then("the coalesced text should be exact")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_SupportsNullCoalescing() => useCase.Cases["Coalesced"].Should().Be("default");

    [Then("the child context text should be exact")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_SupportsChildContext() => useCase.Cases["Child"].Should().Be("Parent - Child");
}
