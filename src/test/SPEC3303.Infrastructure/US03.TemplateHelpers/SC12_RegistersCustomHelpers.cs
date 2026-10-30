namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "12")]
[UserStory("03", "Register adds and overrides custom template helpers")]
public sealed class SC12_RegistersCustomHelpers : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Helper, Func<object?, string?, object?> Register, Func<object?, string?, object?>? Override, string Case, object? Value, string? Argument)[] inputs = [];
    private string existsName = string.Empty;
    private string pipeName = string.Empty;

    public SC12_RegistersCustomHelpers(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("six helper registrations plus one Exists lookup and one pipe template bound to a public context")]
    private void Given()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        inputs = [("testhelper_" + suffix, (value, _) => $"transformed:{value}", null, "Added", "input", null),
            ("testoverride_" + suffix, (_, _) => "first", (_, _) => "second", "Overridden", "input", null),
            ("testwitharg_" + suffix, (value, argument) => $"{value}:{argument}", null, "WithArgument", "hello", "world"),
            ("testnull_" + suffix, (value, _) => value == null ? "was null" : "not null", null, "NullValue", null, null),
            ("testreturnnull_" + suffix, (_, _) => null, null, "ReturnedNull", "input", null),
            ("testtype_" + suffix, (value, _) => int.TryParse(value?.ToString(), out var number) ? number * 2 : 0, null, "DifferentType", "21", null)];
        existsName = "testexists_" + suffix;
        pipeName = "testpipe_" + suffix;
    }

    [When("each helper is registered once and observed through Apply, Exists or a pipe template")]
    private void When()
    {
        foreach (var input in inputs)
        {
            global::LowlandTech.TinyTools.TemplateHelpers.Register(input.Helper, input.Register);
            if (input.Override is not null)
            {
                global::LowlandTech.TinyTools.TemplateHelpers.Register(input.Helper, input.Override);
            }
            useCase.Cases[input.Case] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, input.Helper, input.Argument);
        }
        global::LowlandTech.TinyTools.TemplateHelpers.Register(existsName, (value, _) => value);
        useCase.Cases["AccessibleByExists"] = global::LowlandTech.TinyTools.TemplateHelpers.Exists(existsName);
        global::LowlandTech.TinyTools.TemplateHelpers.Register(pipeName, (value, _) => value?.ToString()?.ToUpperInvariant() + "!");
        var context = new global::LowlandTech.TinyTools.ExecutionContext();
        context.Set("Name", "hello");
        useCase.Cases["PipeTemplate"] = new VariableResolver().ResolveString($"${{Context.Name | {pipeName}}}", context);
    }

    [Then("Result.Added should be transformed_input")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_Added() => useCase.Cases["Added"].Should().Be("transformed:input");

    [Then("Result.Overridden should be second")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_Overridden() => useCase.Cases["Overridden"].Should().Be("second");

    [Then("Result.WithArgument should be hello:world")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_WithArgument() => useCase.Cases["WithArgument"].Should().Be("hello:world");

    [Then("Result.AccessibleByExists should be true")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_AccessibleByExists() => useCase.Cases["AccessibleByExists"].Should().Be(true);

    [Then("Result.PipeTemplate should be HELLO!")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_PipeTemplate() => useCase.Cases["PipeTemplate"].Should().Be("HELLO!");

    [Then("Result.NullValue should be was_null")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_NullValue() => useCase.Cases["NullValue"].Should().Be("was null");

    [Then("Result.ReturnedNull should be null")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ReturnedNull() => useCase.Cases["ReturnedNull"].Should().BeNull();

    [Then("Result.DifferentType should be 42")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_DifferentType() => useCase.Cases["DifferentType"].Should().Be(42);
}
