namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "08")]
[UserStory("03", "Count and first helpers read sequences and scalars")]
public sealed class SC08_CountsSequences : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value, string Helper)[] inputs = [];

    public SC08_CountsSequences(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one value for every count and first case")]
    private void Given()
    {
        inputs = [("CountNull", null, "count"), ("CountString", "hello", "count"),
            ("CountList", new List<int> { 1, 2, 3 }, "count"), ("CountEnumerable", Enumerable.Range(1, 5), "count"),
            ("CountScalar", 42, "count"), ("CountArray", new[] { "a", "b", "c", "d" }, "count"),
            ("FirstNull", null, "first"), ("FirstString", "hello", "first"),
            ("FirstEmptyString", "", "first"), ("FirstList", new List<string> { "apple", "banana", "cherry" }, "first"),
            ("FirstEmptyList", new List<string>(), "first"), ("FirstScalar", 42, "first"),
            ("FirstArray", new[] { 10, 20, 30 }, "first")];
    }

    [When("each count and first input is applied once and the observed result is recorded")]
    private void When()
    {
        foreach (var input in inputs)
        {
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, input.Helper, null);
        }
    }

    [Then("Result.CountNull should be 0")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_CountNull() => useCase.Cases["CountNull"].Should().Be(0);

    [Then("Result.CountString should be 5")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CountString() => useCase.Cases["CountString"].Should().Be(5);

    [Then("Result.CountList should be 3")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_CountList() => useCase.Cases["CountList"].Should().Be(3);

    [Then("Result.CountEnumerable should be 5")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_CountEnumerable() => useCase.Cases["CountEnumerable"].Should().Be(5);

    [Then("Result.CountScalar should be 1")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_CountScalar() => useCase.Cases["CountScalar"].Should().Be(1);

    [Then("Result.CountArray should be 4")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_CountArray() => useCase.Cases["CountArray"].Should().Be(4);

    [Then("Result.FirstNull should be null")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_FirstNull() => useCase.Cases["FirstNull"].Should().BeNull();

    [Then("Result.FirstString should be h")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_FirstString() => useCase.Cases["FirstString"].Should().Be("h");

    [Then("Result.FirstEmptyString should be null")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_FirstEmptyString() => useCase.Cases["FirstEmptyString"].Should().BeNull();

    [Then("Result.FirstList should be apple")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_FirstList() => useCase.Cases["FirstList"].Should().Be("apple");

    [Then("Result.FirstEmptyList should be null")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_FirstEmptyList() => useCase.Cases["FirstEmptyList"].Should().BeNull();

    [Then("Result.FirstScalar should be 42")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_FirstScalar() => useCase.Cases["FirstScalar"].Should().Be(42);

    [Then("Result.FirstArray should be 10")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_FirstArray() => useCase.Cases["FirstArray"].Should().Be(10);
}
