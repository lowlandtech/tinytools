namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "06")]
[UserStory("03", "Default and ifempty helpers fall back for empty content")]
public sealed class SC06_AppliesDefaultFallbacks : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value, string Helper, string? Argument)[] inputs = [];
    private (string Name, string Template, global::LowlandTech.TinyTools.ExecutionContext Context)[] pipes = [];
    private (string Name, string Helper)[] helpers = [];

    public SC06_AppliesDefaultFallbacks(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("empty, populated and missing values for default and ifempty plus the installed helper names")]
    private void Given()
    {
        var missing = new global::LowlandTech.TinyTools.ExecutionContext();
        var named = new global::LowlandTech.TinyTools.ExecutionContext(); named.Set("Name", "John");
        inputs = [("DefaultNull", null, "default", "fallback"), ("DefaultEmptyString", "", "default", "fallback"),
            ("DefaultNonEmpty", "hello", "default", "fallback"), ("DefaultEmptyCollection", new List<string>(), "default", "empty list"),
            ("DefaultCollection", new List<string> { "item1", "item2" }, "default", "empty list"),
            ("DefaultContent", "content", "default", "default value"), ("IfEmptyNull", null, "ifempty", "fallback")];
        pipes = [("DefaultPipeMissing", "${Context.Missing | default:N/A}", missing),
            ("DefaultPipePresent", "${Context.Name | default:Unknown}", named)];
        helpers = [("DefaultExists", "default"), ("IfEmptyExists", "ifempty"), ("RoundExists", "round")];
    }

    [When("each fallback input is applied once, each pipe is resolved once and each name is looked up once")]
    private void When()
    {
        var resolver = new VariableResolver();
        foreach (var input in inputs)
        {
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, input.Helper, input.Argument);
        }
        foreach (var pipe in pipes)
        {
            useCase.Cases[pipe.Name] = resolver.ResolveString(pipe.Template, pipe.Context);
        }
        foreach (var helper in helpers)
        {
            useCase.Cases[helper.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Exists(helper.Helper);
        }
    }

    [Then("Result.DefaultNull should be fallback")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DefaultNull() => useCase.Cases["DefaultNull"].Should().Be("fallback");

    [Then("Result.DefaultEmptyString should be fallback")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DefaultEmptyString() => useCase.Cases["DefaultEmptyString"].Should().Be("fallback");

    [Then("Result.DefaultNonEmpty should be hello")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_DefaultNonEmpty() => useCase.Cases["DefaultNonEmpty"].Should().Be("hello");

    [Then("Result.DefaultEmptyCollection should be empty_list")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DefaultEmptyCollection() => useCase.Cases["DefaultEmptyCollection"].Should().Be("empty list");

    [Then("Result.DefaultCollection should be item1_item2")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_DefaultCollection() => useCase.Cases["DefaultCollection"].Should().BeEquivalentTo(new List<string> { "item1", "item2" });

    [Then("Result.DefaultContent should be content")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_DefaultContent() => useCase.Cases["DefaultContent"].Should().Be("content");

    [Then("Result.DefaultPipeMissing should be N/A")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_DefaultPipeMissing() => useCase.Cases["DefaultPipeMissing"].Should().Be("N/A");

    [Then("Result.DefaultPipePresent should be John")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_DefaultPipePresent() => useCase.Cases["DefaultPipePresent"].Should().Be("John");

    [Then("Result.IfEmptyNull should be fallback")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_IfEmptyNull() => useCase.Cases["IfEmptyNull"].Should().Be("fallback");

    [Then("Result.DefaultExists should be true")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_DefaultExists() => useCase.Cases["DefaultExists"].Should().Be(true);

    [Then("Result.IfEmptyExists should be true")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_IfEmptyExists() => useCase.Cases["IfEmptyExists"].Should().Be(true);

    [Then("Result.RoundExists should be true")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_RoundExists() => useCase.Cases["RoundExists"].Should().Be(true);
}
