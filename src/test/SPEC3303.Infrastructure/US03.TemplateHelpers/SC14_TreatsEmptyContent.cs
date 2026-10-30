namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "14")]
[UserStory("03", "Ifempty treats empty content and scalars consistently")]
public sealed class SC14_TreatsEmptyContent : IClassFixture<UseCases.TemplateHelpersUseCase>
{
    private readonly UseCases.TemplateHelpersUseCase useCase;
    private (string Name, object? Value)[] inputs = [];
    private DateTime dated = DateTime.Now;

    public SC14_TreatsEmptyContent(UseCases.TemplateHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one null, empty, populated and scalar value for every ifempty case")]
    private void Given()
    {
        dated = DateTime.Now;
        inputs = [("IsEmptyNull", null), ("IsEmptyEmptyString", ""),
            ("IsEmptyNonEmptyString", "hello"), ("IsEmptyEmptyCollection", new List<int>()),
            ("IsEmptyCollection", new List<int> { 1, 2, 3 }), ("IsEmptyEmptyEnumerable", Enumerable.Empty<int>()),
            ("IsEmptyEnumerable", Enumerable.Range(1, 5)), ("IsEmptyScalar", 42), ("IsEmptyDate", dated)];
    }

    [When("each ifempty input is applied once and the observed result is recorded")]
    private void When()
    {
        foreach (var input in inputs)
        {
            useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, "ifempty", "empty");
        }
        useCase.Cases["IsEmptyDateSource"] = dated;
    }

    [Then("Result.IsEmptyNull should be empty")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_IsEmptyNull() => useCase.Cases["IsEmptyNull"].Should().Be("empty");

    [Then("Result.IsEmptyEmptyString should be empty")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_IsEmptyEmptyString() => useCase.Cases["IsEmptyEmptyString"].Should().Be("empty");

    [Then("Result.IsEmptyNonEmptyString should be hello")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_IsEmptyNonEmptyString() => useCase.Cases["IsEmptyNonEmptyString"].Should().Be("hello");

    [Then("Result.IsEmptyEmptyCollection should be empty")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_IsEmptyEmptyCollection() => useCase.Cases["IsEmptyEmptyCollection"].Should().Be("empty");

    [Then("Result.IsEmptyCollection should be 1_2_3")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_IsEmptyCollection() => useCase.Cases["IsEmptyCollection"].Should().BeEquivalentTo(new List<int> { 1, 2, 3 });

    [Then("Result.IsEmptyEmptyEnumerable should be empty")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_IsEmptyEmptyEnumerable() => useCase.Cases["IsEmptyEmptyEnumerable"].Should().Be("empty");

    [Then("Result.IsEmptyEnumerable should be 1_2_3_4_5")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_IsEmptyEnumerable() => useCase.Cases["IsEmptyEnumerable"].Should().BeEquivalentTo(Enumerable.Range(1, 5));

    [Then("Result.IsEmptyScalar should be 42")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_IsEmptyScalar() => useCase.Cases["IsEmptyScalar"].Should().Be(42);

    [Then("Result.IsEmptyDate should be dated")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_IsEmptyDate() => useCase.Cases["IsEmptyDate"].Should().Be(useCase.Cases["IsEmptyDateSource"]);
}
