namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "05")]
[UserStory("03", "Rounding helper keeps the numeric type of its input")]
public sealed class SC05_RoundsNumericValues : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value, string? Argument)[] inputs = [];
    private string pipeTemplate = string.Empty;
    private global::LowlandTech.TinyTools.ExecutionContext pipeContext = null!;

    public SC05_RoundsNumericValues(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a numeric value and decimal argument for every rounding case plus one pipe template")]
    private void Given()
    {
        pipeTemplate = "${Context.Value | round:2}";
        pipeContext = new global::LowlandTech.TinyTools.ExecutionContext();
        pipeContext.Set("Value", 3.14159);
        inputs = [("RoundDouble", 3.14159, "2"), ("RoundDecimal", 3.14159m, "2"),
            ("RoundFloat", 3.14159f, "2"), ("RoundDefault", 3.7, null),
            ("RoundNull", null, "2"), ("RoundNonNumeric", "not a number", "2"),
            ("RoundDecimalZero", 5.999m, "0"), ("RoundFloatUp", 2.5f, "0")];
    }

    [When("each rounding input is applied once and the pipe template is resolved once")]
    private void When()
    {
        foreach (var input in inputs)
        {
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, "round", input.Argument);
        }
        useCase.Cases["RoundPipe"] = new VariableResolver().ResolveString(pipeTemplate, pipeContext);
    }

    [Then("Result.RoundDouble should be 3.14")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RoundDouble() => useCase.Cases["RoundDouble"].Should().Be(3.14);

    [Then("Result.RoundDecimal should be 3.14m")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RoundDecimal() => useCase.Cases["RoundDecimal"].Should().Be(3.14m);

    [Then("Result.RoundFloat should be 3.14")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RoundFloat() => ((double)useCase.Cases["RoundFloat"]!).Should().BeApproximately(3.14, 0.01);

    [Then("Result.RoundDefault should be 4.0")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RoundDefault() => useCase.Cases["RoundDefault"].Should().Be(4.0);

    [Then("Result.RoundNull should be null")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RoundNull() => useCase.Cases["RoundNull"].Should().BeNull();

    [Then("Result.RoundNonNumeric should be not_a_number")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RoundNonNumeric() => useCase.Cases["RoundNonNumeric"].Should().Be("not a number");

    [Then("Result.RoundDecimalZero should be 6m")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RoundDecimalZero() => useCase.Cases["RoundDecimalZero"].Should().Be(6m);

    [Then("Result.RoundFloatUp should be 2.0")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RoundFloatUp() => ((double)useCase.Cases["RoundFloatUp"]!).Should().BeApproximately(2.0, 0.01);

    [Then("Result.RoundPipe should be 3.14")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_RoundPipe() => double.Parse((string)useCase.Cases["RoundPipe"]!, System.Globalization.CultureInfo.CurrentCulture).Should().BeApproximately(3.14, 0.001);
}
