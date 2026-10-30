namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait("SPEC", "3303"), Trait("US", "03"), Trait("SC", "03")]
[UserStory("03", "Additional helper null, argument and timeout boundaries remain explicit")]
public sealed class SC03_AdditionalHelperBoundaries : IClassFixture<UseCases.AdditionalHelpersUseCase>
{
    private readonly UseCases.AdditionalHelpersUseCase useCase;
    private (string Name, object? Value, string Helper, string? Argument)[] inputs = [];

    public SC03_AdditionalHelperBoundaries(UseCases.AdditionalHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("null values, invalid arguments, an exact end boundary and a bounded catastrophic regex input")]
    private void Given()
    {
        inputs = [("NullSlice", null, "slice", "0,0"), ("NullMatch", null, "match", "a"),
            ("NullContains", null, "contains", "a"), ("NullSplit", null, "split", ";"),
            ("NumericCollection", new object[] { 7 }, "contains", "7"),
            ("EndSlice", "abc", "slice", "3,3"), ("ReversedSlice", "abc", "slice", "2,1"),
            ("TimedOut", new string('a', 10000) + "!", "match", "(a+)+$"),
            ("MissingSlice", "abc", "slice", null), ("MissingMatch", "abc", "match", null),
            ("MissingContains", "abc", "contains", null), ("MissingSplit", "abc", "split", null)];
    }

    [When("the public helper API evaluates each independently constructed boundary input")]
    private void When()
    {
        foreach (var input in inputs)
        {
            try { useCase.Cases[input.Name] = global::LowlandTech.TinyTools.TemplateHelpers.Apply(input.Value, input.Helper, input.Argument); }
            catch (Exception error) when (error is ArgumentException or System.Text.RegularExpressions.RegexMatchTimeoutException)
            { useCase.Cases[input.Name] = error.GetType().Name; }
        }
    }

    [Then("Result.NullSlice should be null")]
    [Trait("UAC", "01"), Fact(Skip = "TODO: implement and execute the NullSlice boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC01_NullSlice() => useCase.Cases["NullSlice"].Should().BeNull();

    [Then("Result.NullMatch should be false")]
    [Trait("UAC", "02"), Fact(Skip = "TODO: implement and execute the NullMatch boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC02_NullMatch() => useCase.Cases["NullMatch"].Should().Be(false);

    [Then("Result.NullContains should be false")]
    [Trait("UAC", "03"), Fact(Skip = "TODO: implement and execute the NullContains boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC03_NullContains() => useCase.Cases["NullContains"].Should().Be(false);

    [Then("Result.NullSplit should be empty_array")]
    [Trait("UAC", "04"), Fact(Skip = "TODO: implement and execute the NullSplit boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC04_NullSplit() => useCase.Cases["NullSplit"].Should().BeEquivalentTo(Array.Empty<string>());

    [Then("Result.NumericCollection should be false")]
    [Trait("UAC", "05"), Fact(Skip = "TODO: implement and execute the NumericCollection boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC05_NumericCollection() => useCase.Cases["NumericCollection"].Should().Be(false);

    [Then("Result.EndSlice should be empty")]
    [Trait("UAC", "06"), Fact(Skip = "TODO: implement and execute the EndSlice boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC06_EndSlice() => useCase.Cases["EndSlice"].Should().Be("");

    [Then("Result.ReversedSlice should be ArgumentOutOfRangeException")]
    [Trait("UAC", "07"), Fact(Skip = "TODO: implement and execute the ReversedSlice boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC07_ReversedSlice() => useCase.Cases["ReversedSlice"].Should().Be("ArgumentOutOfRangeException");

    [Then("Result.TimedOut should be RegexMatchTimeoutException")]
    [Trait("UAC", "08"), Fact(Skip = "TODO: implement and execute the TimedOut boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC08_TimedOut() => useCase.Cases["TimedOut"].Should().Be("RegexMatchTimeoutException");

    [Then("Result.MissingSlice should be ArgumentException")]
    [Trait("UAC", "09"), Fact(Skip = "TODO: implement and execute the MissingSlice boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC09_MissingSlice() => useCase.Cases["MissingSlice"].Should().Be("ArgumentException");

    [Then("Result.MissingMatch should be ArgumentException")]
    [Trait("UAC", "10"), Fact(Skip = "TODO: implement and execute the MissingMatch boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC10_MissingMatch() => useCase.Cases["MissingMatch"].Should().Be("ArgumentException");

    [Then("Result.MissingContains should be ArgumentException")]
    [Trait("UAC", "11"), Fact(Skip = "TODO: implement and execute the MissingContains boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC11_MissingContains() => useCase.Cases["MissingContains"].Should().Be("ArgumentException");

    [Then("Result.MissingSplit should be ArgumentException")]
    [Trait("UAC", "12"), Fact(Skip = "TODO: implement and execute the MissingSplit boundary through public Apply; no accepted behavior is claimed.")]
    public void UAC12_MissingSplit() => useCase.Cases["MissingSplit"].Should().Be("ArgumentException");
}
