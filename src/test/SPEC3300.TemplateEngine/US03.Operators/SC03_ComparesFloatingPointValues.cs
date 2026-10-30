namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US03.Operators;

[Trait("SPEC", "3300"), Trait("US", "03"), Trait("SC", "03")]
[UserStory("03", "Numeric tolerance keeps materially different operands unequal")]
public sealed class SC03_ComparesFloatingPointValues : IClassFixture<UseCases.ComparisonBoundaryUseCase>
{
    private readonly UseCases.ComparisonBoundaryUseCase useCase;
    private TinyTemplateEngine _engine = null!;
    private (string Name, object Left, object Right)[] _cases = [];

    public SC03_ComparesFloatingPointValues(UseCases.ComparisonBoundaryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("roundoff, inside, exact and outside absolute tolerance operands plus integer and string controls")]
    private void Given()
    {
        _engine = new TinyTemplateEngine();
        _cases = [("roundoff", 0.1 + 0.2, 0.3), ("inside", 0d, Math.BitDecrement(1e-10)),
            ("boundary", 0d, 1e-10), ("outside", 0d, Math.BitIncrement(1e-10)),
            ("integer", 7, 7), ("string", "Alpha", "ALPHA")];
    }

    [When("the public engine renders equality for each independently constructed context")]
    private void When()
    {
        foreach (var item in _cases)
        {
            var context = new global::LowlandTech.TinyTools.ExecutionContext();
            context.Set("Left", item.Left);
            context.Set("Right", item.Right);
            useCase.Cases[item.Name] = new[] { _engine.Render("@if (Context.Left == Context.Right) {\nT\n}", context).Trim() };
        }
        useCase.Cases["ToleranceBoundary"] = ((string[])useCase.Cases["inside"]!).Concat((string[])useCase.Cases["boundary"]!).Concat((string[])useCase.Cases["outside"]!).ToArray();
        useCase.Cases["Controls"] = ((string[])useCase.Cases["integer"]!).Concat((string[])useCase.Cases["string"]!).ToArray();
    }

    [Then("Result.Roundoff should be T")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_Roundoff() => ((string[])useCase.Cases["roundoff"]!).Should().Equal("T");

    [Then("Result.ToleranceBoundary should be [T,empty,empty]")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ToleranceBoundary() => ((string[])useCase.Cases["ToleranceBoundary"]!).Should().Equal("T", "", "");

    [Then("Result.Controls should be [T,T]")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_Controls() => ((string[])useCase.Cases["Controls"]!).Should().Equal("T", "T");
}
