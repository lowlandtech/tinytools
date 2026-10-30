namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "09")]
[UserStory("03", "Last and reverse helpers read and reorder sequences")]
public sealed class SC09_ReadsLastAndReverse : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value, string Helper)[] inputs = [];

    public SC09_ReadsLastAndReverse(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one value for every last and reverse case")]
    private void Given()
    {
        inputs = [("LastNull", null, "last"), ("LastString", "hello", "last"),
            ("LastEmptyString", "", "last"), ("LastList", new List<string> { "apple", "banana", "cherry" }, "last"),
            ("LastEmptyList", new List<string>(), "last"), ("LastScalar", 42, "last"),
            ("LastArray", new[] { 10, 20, 30 }, "last"), ("ReverseNull", null, "reverse"),
            ("ReverseString", "hello", "reverse"), ("ReverseList", new List<int> { 1, 2, 3 }, "reverse"),
            ("ReverseScalar", 42, "reverse"), ("ReverseArray", new[] { "a", "b", "c" }, "reverse"),
            ("ReverseEmptyString", "", "reverse"), ("ReverseSingleChar", "x", "reverse")];
    }

    [When("each last and reverse input is applied once and the observed result is recorded")]
    private void When()
    {
        foreach (var input in inputs)
        {
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, input.Helper, null);
        }
    }

    [Then("Result.LastNull should be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_LastNull() => useCase.Cases["LastNull"].Should().BeNull();

    [Then("Result.LastString should be o")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_LastString() => useCase.Cases["LastString"].Should().Be("o");

    [Then("Result.LastEmptyString should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_LastEmptyString() => useCase.Cases["LastEmptyString"].Should().BeNull();

    [Then("Result.LastList should be cherry")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_LastList() => useCase.Cases["LastList"].Should().Be("cherry");

    [Then("Result.LastEmptyList should be null")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_LastEmptyList() => useCase.Cases["LastEmptyList"].Should().BeNull();

    [Then("Result.LastScalar should be 42")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_LastScalar() => useCase.Cases["LastScalar"].Should().Be(42);

    [Then("Result.LastArray should be 30")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_LastArray() => useCase.Cases["LastArray"].Should().Be(30);

    [Then("Result.ReverseNull should be null")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_ReverseNull() => useCase.Cases["ReverseNull"].Should().BeNull();

    [Then("Result.ReverseString should be olleh")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_ReverseString() => useCase.Cases["ReverseString"].Should().Be("olleh");

    [Then("Result.ReverseList should be 3_2_1")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_ReverseList() => useCase.Cases["ReverseList"].Should().BeEquivalentTo(new List<int> { 3, 2, 1 });

    [Then("Result.ReverseScalar should be 42")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_ReverseScalar() => useCase.Cases["ReverseScalar"].Should().Be(42);

    [Then("Result.ReverseArray should be c_b_a")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_ReverseArray() => useCase.Cases["ReverseArray"].Should().BeEquivalentTo(new List<string> { "c", "b", "a" });

    [Then("Result.ReverseEmptyString should be empty")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_ReverseEmptyString() => useCase.Cases["ReverseEmptyString"].Should().Be("");

    [Then("Result.ReverseSingleChar should be x")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_ReverseSingleChar() => useCase.Cases["ReverseSingleChar"].Should().Be("x");
}
