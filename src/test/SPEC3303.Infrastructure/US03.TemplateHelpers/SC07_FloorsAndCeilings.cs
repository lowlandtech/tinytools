namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "07")]
[UserStory("03", "Floor and ceiling helpers keep the numeric type of their input")]
public sealed class SC07_FloorsAndCeilings : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value, string Helper)[] inputs = [];

    public SC07_FloorsAndCeilings(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one value for every floor and ceiling case")]
    private void Given()
    {
        inputs = [("FloorDouble", 3.7, "floor"), ("FloorDecimal", 3.7m, "floor"),
            ("FloorFloat", 3.7f, "floor"), ("FloorNonNumeric", "text", "floor"),
            ("FloorNegative", -3.2, "floor"), ("CeilingDouble", 3.2, "ceiling"),
            ("CeilingDecimal", 3.2m, "ceiling"), ("CeilingFloat", 3.2f, "ceiling"),
            ("CeilingNonNumeric", "text", "ceiling"), ("CeilingNegative", -3.7, "ceiling")];
    }

    [When("each floor and ceiling input is applied once and the observed result is recorded")]
    private void When()
    {
        foreach (var input in inputs)
        {
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, input.Helper, null);
        }
    }

    [Then("Result.FloorDouble should be 3.0")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_FloorDouble() => useCase.Cases["FloorDouble"].Should().Be(3.0);

    [Then("Result.FloorDecimal should be 3m")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_FloorDecimal() => useCase.Cases["FloorDecimal"].Should().Be(3m);

    [Then("Result.FloorFloat should be 3.0")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_FloorFloat() => ((double)useCase.Cases["FloorFloat"]!).Should().BeApproximately(3.0, 0.01);

    [Then("Result.FloorNonNumeric should be text")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_FloorNonNumeric() => useCase.Cases["FloorNonNumeric"].Should().Be("text");

    [Then("Result.FloorNegative should be -4.0")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_FloorNegative() => useCase.Cases["FloorNegative"].Should().Be(-4.0);

    [Then("Result.CeilingDouble should be 4.0")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_CeilingDouble() => useCase.Cases["CeilingDouble"].Should().Be(4.0);

    [Then("Result.CeilingDecimal should be 4m")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_CeilingDecimal() => useCase.Cases["CeilingDecimal"].Should().Be(4m);

    [Then("Result.CeilingFloat should be 4.0")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_CeilingFloat() => ((double)useCase.Cases["CeilingFloat"]!).Should().BeApproximately(4.0, 0.01);

    [Then("Result.CeilingNonNumeric should be text")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_CeilingNonNumeric() => useCase.Cases["CeilingNonNumeric"].Should().Be("text");

    [Then("Result.CeilingNegative should be -3.0")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_CeilingNegative() => useCase.Cases["CeilingNegative"].Should().Be(-3.0);
}
