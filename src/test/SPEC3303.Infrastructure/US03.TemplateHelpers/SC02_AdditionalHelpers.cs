namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "02")]
[UserStory("03", "Additional pipe helpers have deterministic success and refusal results")]
public sealed class SC02_AdditionalHelpers : IClassFixture<UseCases.AdditionalHelpersUseCase>
{
    private readonly UseCases.AdditionalHelpersUseCase useCase;
    private (string Name, object? Value, string Helper, string? Argument)[] inputs = [];
    private TinyTemplateEngine engine = null!;
    private global::LowlandTech.TinyTools.ExecutionContext context = null!;

    public SC02_AdditionalHelpers(UseCases.AdditionalHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("public helper inputs and a separate public engine context for chained split and count")]
    private void Given()
    {
        engine = new TinyTemplateEngine();
        context = new global::LowlandTech.TinyTools.ExecutionContext();
        context.Set("Value", "a;;b;");
        inputs = [("Slice", "abcde", "slice", "1,4"), ("EmptySlice", "abc", "slice", "2,2"),
            ("SliceBounds", "abc", "slice", "-1,2"), ("SliceArgument", "abc", "slice", "x,2"),
            ("Match", "abc12", "match", "^[a-z]+[0-9]+$"), ("BadPattern", "abc", "match", "["),
            ("Contains", "Alpha", "contains", "ph"), ("ContainsCase", "Alpha", "contains", "PH"),
            ("Collection", new[] { "A", "b" }, "contains", "A"),
            ("Split", "a;;b;", "split", ";"), ("EmptySeparator", "abc", "split", "")];
    }

    [When("each public helper is called and the public engine renders split followed by count")]
    private void When()
    {
        foreach (var input in inputs)
        {
            try { useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, input.Helper, input.Argument); }
            catch (ArgumentException error) { useCase.Cases[input.Name] = error.GetType().Name; }
        }
        useCase.Cases["ChainedCount"] = engine.Render("${Context.Value | split:; | count}", context);
    }

    [Then("Result.Slice should be bcd")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_Slice() => useCase.Cases["Slice"].Should().Be("bcd");

    [Then("Result.EmptySlice should be empty")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_EmptySlice() => useCase.Cases["EmptySlice"].Should().Be("");

    [Then("Result.SliceBounds should be ArgumentOutOfRangeException")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_SliceBounds() => useCase.Cases["SliceBounds"].Should().Be("ArgumentOutOfRangeException");

    [Then("Result.SliceArgument should be ArgumentException")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_SliceArgument() => useCase.Cases["SliceArgument"].Should().Be("ArgumentException");

    [Then("Result.Match should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_Match() => useCase.Cases["Match"].Should().Be(true);

    [Then("Result.BadPattern should be ArgumentException")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_BadPattern() => useCase.Cases["BadPattern"].Should().Be("ArgumentException");

    [Then("Result.Contains should be true")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_Contains() => useCase.Cases["Contains"].Should().Be(true);

    [Then("Result.ContainsCase should be false")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_ContainsCase() => useCase.Cases["ContainsCase"].Should().Be(false);

    [Then("Result.Collection should be true")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_Collection() => useCase.Cases["Collection"].Should().Be(true);

    [Then("Result.Split should be a_empty_b_empty")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_Split() => useCase.Cases["Split"].Should().BeEquivalentTo(new[] { "a", "", "b", "" }, options => options.WithStrictOrdering());

    [Then("Result.EmptySeparator should be ArgumentException")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_EmptySeparator() => useCase.Cases["EmptySeparator"].Should().Be("ArgumentException");

    [Then("Result.ChainedCount should be 4")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_ChainedCount() => useCase.Cases["ChainedCount"].Should().Be("4");
}
