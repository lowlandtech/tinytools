namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "11")]
[UserStory("03", "Format helper falls back for unformattable and missing arguments")]
public sealed class SC11_FormatsRemainingValues : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value, string? Argument)[] inputs = [];

    public SC11_FormatsRemainingValues(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one value and a missing, empty or custom format argument for every remaining format case")]
    private void Given()
    {
        inputs = [("FormatNonFormattable", new { Name = "Test" }, "any"),
            ("FormatNullArgument", new DateTime(2024, 3, 15), null),
            ("FormatEmptyArgument", new DateTime(2024, 3, 15), ""),
            ("FormatNullValue", null, "yyyy-MM-dd"), ("FormatInteger", 42, "D5"),
            ("FormatCustom", new DateTime(2024, 3, 15, 14, 30, 0), "MMMM dd, yyyy")];
    }

    [When("each remaining format input is applied once and the observed result is recorded")]
    private void When()
    {
        foreach (var input in inputs)
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, "format", input.Argument);
    }

    [Then("Result.FormatNonFormattable should contain Name")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_FormatNonFormattable() => useCase.Cases["FormatNonFormattable"]!.ToString().Should().Contain("Name");

    [Then("Result.FormatNullArgument should not be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_FormatNullArgument() => useCase.Cases["FormatNullArgument"].Should().NotBeNull();

    [Then("Result.FormatNullArgumentText should contain 2024")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_FormatNullArgumentText() => useCase.Cases["FormatNullArgument"]!.ToString().Should().Contain("2024");

    [Then("Result.FormatEmptyArgument should not be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_FormatEmptyArgument() => useCase.Cases["FormatEmptyArgument"].Should().NotBeNull();

    [Then("Result.FormatEmptyArgumentText should contain 2024")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_FormatEmptyArgumentText() => useCase.Cases["FormatEmptyArgument"]!.ToString().Should().Contain("2024");

    [Then("Result.FormatNullValue should be null")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_FormatNullValue() => useCase.Cases["FormatNullValue"].Should().BeNull();

    [Then("Result.FormatInteger should be 00042")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_FormatInteger() => useCase.Cases["FormatInteger"].Should().Be("00042");

    [Then("Result.FormatCustom should be March_15_2024")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_FormatCustom() => useCase.Cases["FormatCustom"].Should().Be("March 15, 2024");
}
