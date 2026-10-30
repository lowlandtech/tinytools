namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US03.Operators;

[Trait("SPEC", "3300"), Trait("US", "03"), Trait("SC", "02")]
[UserStory("03", "Comparison operators preserve their equality boundary")]
public sealed class SC02_PreservesComparisonPrecedence : IClassFixture<UseCases.ComparisonBoundaryUseCase>
{
    private readonly UseCases.ComparisonBoundaryUseCase _useCase;
    private TinyTemplateEngine _engine = null!;
    private global::LowlandTech.TinyTools.ExecutionContext _context = null!;
    private string[] _operators = [];

    public SC02_PreservesComparisonPrecedence(UseCases.ComparisonBoundaryUseCase useCase)
    {
        _useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("an engine, equality operand 10 and each strict or inclusive comparison operator")]
    private void Given()
    {
        _engine = new TinyTemplateEngine();
        _context = new global::LowlandTech.TinyTools.ExecutionContext();
        _operators = [">=", ">", "<=", "<"];
    }

    [When("the engine renders the equality and adjacent-boundary conditions")]
    private void When()
    {
        foreach (var left in new[] { 10, 9, 11 })
        {
            _context.Set("Left", left);
            var outputs = new List<string>();
            foreach (var op in _operators)
                outputs.Add(_engine.Render($"@if (Context.Left {op} 10) {{\nT\n}}", _context).Trim());
            _useCase.Cases[left.ToString(System.Globalization.CultureInfo.InvariantCulture)] = outputs.ToArray();
        }
        _useCase.Cases["EqualGreater"] = _useCase.Cases["10"].Take(2).ToArray();
        _useCase.Cases["EqualLess"] = _useCase.Cases["10"].Skip(2).ToArray();
        _useCase.Cases["AdjacentBoundaries"] = _useCase.Cases["9"].Concat(_useCase.Cases["11"]).ToArray();
    }

    [Then("Result.EqualGreater should be [T,empty]")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_GreaterEquality() => _useCase.Cases["EqualGreater"].Should().Equal("T", "");

    [Then("Result.EqualLess should be [T,empty]")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_LessEquality() => _useCase.Cases["EqualLess"].Should().Equal("T", "");

    [Then("Result.AdjacentBoundaries should be [empty,empty,T,T,T,T,empty,empty]")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_AdjacentBoundaries() => _useCase.Cases["AdjacentBoundaries"].Should().Equal("", "", "T", "T", "T", "T", "", "");
}
