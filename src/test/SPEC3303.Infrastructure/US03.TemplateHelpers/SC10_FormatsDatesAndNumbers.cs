namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "10")]
[UserStory("03", "Format helper renders dates, numbers and decimals")]
public sealed class SC10_FormatsDatesAndNumbers : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value, string? Argument)[] inputs = [];

    public SC10_FormatsDatesAndNumbers(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one temporal or numeric value and format argument for every format case")]
    private void Given()
    {
        inputs = [("FormatDateTime", new DateTime(2024, 3, 15, 10, 30, 0), "yyyy-MM-dd"),
            ("FormatDateTimeTime", new DateTime(2024, 3, 15, 10, 30, 45), "HH:mm:ss"),
            ("FormatDateTimeOffset", new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero), "yyyy-MM-dd"),
            ("FormatDateOnly", new DateOnly(2024, 3, 15), "yyyy-MM-dd"),
            ("FormatTimeOnly", new TimeOnly(10, 30, 45), "HH:mm:ss"),
            ("FormatNumber", 1234.567, "F2"), ("FormatDecimal", 1234.5678m, "C2")];
    }

    [When("each format input is applied once and the observed result is recorded")]
    private void When()
    {
        foreach (var input in inputs)
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, "format", input.Argument);
    }

    [Then("Result.FormatDateTime should be 2024-03-15")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_FormatDateTime() => useCase.Cases["FormatDateTime"].Should().Be("2024-03-15");

    [Then("Result.FormatDateTimeTime should be 10:30:45")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_FormatDateTimeTime() => useCase.Cases["FormatDateTimeTime"].Should().Be("10:30:45");

    [Then("Result.FormatDateTimeOffset should be 2024-03-15")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_FormatDateTimeOffset() => useCase.Cases["FormatDateTimeOffset"].Should().Be("2024-03-15");

    [Then("Result.FormatDateOnly should be 2024-03-15")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_FormatDateOnly() => useCase.Cases["FormatDateOnly"].Should().Be("2024-03-15");

    [Then("Result.FormatTimeOnly should be 10:30:45")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_FormatTimeOnly() => useCase.Cases["FormatTimeOnly"].Should().Be("10:30:45");

    [Then("Result.FormatNumber should be 1234.57")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_FormatNumber() => useCase.Cases["FormatNumber"].Should().Be("1234.57");

    [Then("Result.FormatDecimal should not be null")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_FormatDecimal() => useCase.Cases["FormatDecimal"].Should().NotBeNull();

    [Then("Result.FormatDecimalText should be nonempty")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_FormatDecimalText() => useCase.Cases["FormatDecimal"]!.ToString().Should().NotBeEmpty();
}
