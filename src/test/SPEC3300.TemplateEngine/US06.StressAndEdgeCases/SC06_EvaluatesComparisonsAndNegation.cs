using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "06")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC06_EvaluatesComparisonsAndNegation : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context24 = new();
    private readonly ExecutionContext _context25 = new();
    private readonly ExecutionContext _context26 = new();
    private readonly ExecutionContext _context27 = new();
    private readonly ExecutionContext _context28 = new();

    public SC06_EvaluatesComparisonsAndNegation(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with a high count, a boundary count, two statuses and a hidden flag")]
    protected override void Given()
    {
        _context24.Set("Count", 10);
        _context25.Set("Count", 5);
        _context26.Set("Status", "active");
        _context27.Set("Status", "inactive");
        _context28.Set("IsHidden", false);
    }

    [When("each comparison and negation template is rendered once")]
    protected override void When()
    {
        base.When();

        useCase.Cases["GreaterThan"] = Sut.Render("""
            @if (Context.Count > 5) {
            More than 5
            }
            """, _context24);

        useCase.Cases["LessOrEqual"] = Sut.Render("""
            @if (Context.Count <= 5) {
            5 or less
            }
            """, _context25);

        useCase.Cases["StringEquality"] = Sut.Render("""
            @if (Context.Status == "active") {
            Is Active
            }
            """, _context26);

        useCase.Cases["StringInequality"] = Sut.Render("""
            @if (Context.Status != "active") {
            Not Active
            }
            """, _context27);

        useCase.Cases["Negation"] = Sut.Render("""
            @if (!Context.IsHidden) {
            Visible
            }
            """, _context28);
    }

    [Then("the render should contain More than 5")]
    [Trait("UAC", "24"), Fact]
    public void UAC24_HandlesGreaterThanComparison() => ((string)useCase.Cases["GreaterThan"]!).Should().Contain("More than 5");

    [Then("the render should contain 5 or less")]
    [Trait("UAC", "25"), Fact]
    public void UAC25_HandlesLessThanOrEqualComparison() => ((string)useCase.Cases["LessOrEqual"]!).Should().Contain("5 or less");

    [Then("the render should contain Is Active")]
    [Trait("UAC", "26"), Fact]
    public void UAC26_HandlesStringEquality() => ((string)useCase.Cases["StringEquality"]!).Should().Contain("Is Active");

    [Then("the render should contain Not Active")]
    [Trait("UAC", "27"), Fact]
    public void UAC27_HandlesStringInequality() => ((string)useCase.Cases["StringInequality"]!).Should().Contain("Not Active");

    [Then("the render should contain Visible")]
    [Trait("UAC", "28"), Fact]
    public void UAC28_HandlesNegation() => ((string)useCase.Cases["Negation"]!).Should().Contain("Visible");
}
