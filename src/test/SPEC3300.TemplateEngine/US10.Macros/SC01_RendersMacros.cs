namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US10.Macros;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("10", "Templates define and invoke parameterized macros")]
public sealed class SC01_RendersMacros : IClassFixture<UseCases.MacrosUseCase>
{
    private readonly UseCases.MacrosUseCase useCase;
    private TinyTemplateEngine engine = null!;
    private ExecutionContext context = null!;

    public SC01_RendersMacros(UseCases.MacrosUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("an engine, a caller context and the templates for each macro case")]
    private void Given()
    {
        engine = new TinyTemplateEngine();
        context = new ExecutionContext();
        context.Set("Name", "Ada");
        context.Set("Flag", true);
    }

    [When("the public Render executes each template once")]
    private void When()
    {

        useCase.Cases["Macro"] = engine.Render(
            "@macro greet(who, mark) { Hello ${who}${mark} }\n@greet(\"Ada\", \"!\")", context);

        useCase.Cases["Calls"] = engine.Render(
            "@macro tag(value) { [${value}] }\n@tag(\"one\")@tag(\"two\")", context);

        useCase.Cases["Composed"] = engine.Render(
            "@macro block(title) { # ${title}\n@if (Context.Flag) {\nyes\n} }\n@block(\"Head\")", context);

        useCase.Cases["TooFew"] = Thrown(engine, "@macro pair(a, b) { ${a}${b} }\n@pair(\"x\")", context);
        useCase.Cases["TooMany"] = Thrown(engine, "@macro one(a) { ${a} }\n@one(\"x\", \"y\")", context);
        useCase.Cases["Unknown"] = Thrown(engine, "@macro known(a) { ${a} }\n@missing(\"x\")", context);
        useCase.Cases["Recursion"] = Thrown(engine, "@macro loop(a) { @loop(\"x\") }\n@loop(\"x\")", context);
    }

    private static string Thrown(TinyTemplateEngine engine, string template, ExecutionContext context)
    {
        try
        {
            engine.Render(template, context);
            return "no-throw";
        }
        catch (InvalidOperationException error)
        {
            return error.Message;
        }
    }

    [Then("the macro output should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_SubstitutesEachParameter() => ((string)useCase.Cases["Macro"]!).Should().Be("\n Hello Ada! ");

    [Then("the calls output should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_KeepsEachCallIsolated() => ((string)useCase.Cases["Calls"]!).Should().Be("\n [one]  [two] ");

    [Then("the composed output should be exact")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_KeepsInterpolationAndControlFlow() => ((string)useCase.Cases["Composed"]!).Should().Be("\n # Head\nyes");

    [Then("the too few message should contain expects 2 argument")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RefusesTooFewArguments() => ((string)useCase.Cases["TooFew"]!).Should().Contain("expects 2 argument(s) but received 1");

    [Then("the too many message should contain expects 1 argument")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RefusesTooManyArguments() => ((string)useCase.Cases["TooMany"]!).Should().Contain("expects 1 argument(s) but received 2");

    [Then("the unknown message should contain is not defined")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_NamesTheUnknownMacro() => ((string)useCase.Cases["Unknown"]!).Should().Contain("Macro 'missing' is not defined");

    [Then("the recursion message should contain recursion limit")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_StopsRecursion() => ((string)useCase.Cases["Recursion"]!).Should().Contain("Macro recursion limit exceeded");
}
