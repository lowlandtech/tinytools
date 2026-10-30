using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "06")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC06_EvaluatesComparisonsAndNegation : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context24 = new();
    private readonly ExecutionContext _context25 = new();
    private readonly ExecutionContext _context26 = new();
    private readonly ExecutionContext _context27 = new();
    private readonly ExecutionContext _context28 = new();
    private string _result24 = string.Empty;
    private string _result25 = string.Empty;
    private string _result26 = string.Empty;
    private string _result27 = string.Empty;
    private string _result28 = string.Empty;

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

        _result24 = Sut.Render("""
            @if (Context.Count > 5) {
            More than 5
            }
            """, _context24);

        _result25 = Sut.Render("""
            @if (Context.Count <= 5) {
            5 or less
            }
            """, _context25);

        _result26 = Sut.Render("""
            @if (Context.Status == "active") {
            Is Active
            }
            """, _context26);

        _result27 = Sut.Render("""
            @if (Context.Status != "active") {
            Not Active
            }
            """, _context27);

        _result28 = Sut.Render("""
            @if (!Context.IsHidden) {
            Visible
            }
            """, _context28);
    }

    public SC06_EvaluatesComparisonsAndNegation() => ArrangeAndAct();

    [Then("the render should contain More than 5")]
    [Trait("UAC", "24"), Fact]
    public void UAC24_HandlesGreaterThanComparison() => _result24.Should().Contain("More than 5");

    [Then("the render should contain 5 or less")]
    [Trait("UAC", "25"), Fact]
    public void UAC25_HandlesLessThanOrEqualComparison() => _result25.Should().Contain("5 or less");

    [Then("the render should contain Is Active")]
    [Trait("UAC", "26"), Fact]
    public void UAC26_HandlesStringEquality() => _result26.Should().Contain("Is Active");

    [Then("the render should contain Not Active")]
    [Trait("UAC", "27"), Fact]
    public void UAC27_HandlesStringInequality() => _result27.Should().Contain("Not Active");

    [Then("the render should contain Visible")]
    [Trait("UAC", "28"), Fact]
    public void UAC28_HandlesNegation() => _result28.Should().Contain("Visible");
}
