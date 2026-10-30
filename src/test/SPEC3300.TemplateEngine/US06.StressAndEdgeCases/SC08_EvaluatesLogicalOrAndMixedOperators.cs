using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "08")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC08_EvaluatesLogicalOrAndMixedOperators : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context33 = new();
    private readonly ExecutionContext _context34 = new();
    private readonly ExecutionContext _context35 = new();
    private readonly ExecutionContext _context36 = new();
    private string _result33 = string.Empty;
    private string _result34 = string.Empty;
    private string _result35 = string.Empty;
    private string _result36 = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with no items, an administrator, a licensed adult and a verified account")]
    protected override void Given()
    {
        _context33.Set("HasItems", false);
        _context33.Set("ShowEmpty", false);
        _context34.Set("IsAdmin", true);
        _context34.Set("IsModerator", false);
        _context34.Set("IsOwner", true);
        _context35.Set("Age", 25);
        _context35.Set("HasLicense", true);
        _context36.Set("IsBlocked", false);
        _context36.Set("IsVerified", true);
    }

    [When("each disjunction and mixed operator template is rendered once")]
    protected override void When()
    {
        base.When();

        _result33 = Sut.Render("""
            @if (Context.HasItems || Context.ShowEmpty) {
            Show Container
            } else {
            Hidden
            }
            """, _context33);

        _result34 = Sut.Render("""
            @if (Context.IsAdmin || Context.IsModerator && Context.IsOwner) {
            Has Access
            }
            """, _context34);

        _result35 = Sut.Render("""
            @if (Context.Age >= 18 && Context.HasLicense) {
            Can Drive
            }
            """, _context35);

        _result36 = Sut.Render("""
            @if (!Context.IsBlocked && Context.IsVerified) {
            Account Active
            }
            """, _context36);
    }

    public SC08_EvaluatesLogicalOrAndMixedOperators() => ArrangeAndAct();

    [Then("the render should contain Hidden")]
    [Trait("UAC", "33"), Fact]
    public void UAC33_HandlesLogicalOrBothFalse() => _result33.Should().Contain("Hidden").And.NotContain("Show Container");

    [Then("the render should contain Has Access")]
    [Trait("UAC", "34"), Fact]
    public void UAC34_HandlesMixedLogicalOperators() => _result34.Should().Contain("Has Access");

    [Then("the render should contain Can Drive")]
    [Trait("UAC", "35"), Fact]
    public void UAC35_HandlesLogicalOperatorsWithComparisons() => _result35.Should().Contain("Can Drive");

    [Then("the render should contain Account Active")]
    [Trait("UAC", "36"), Fact]
    public void UAC36_HandlesLogicalOperatorsWithNegation() => _result36.Should().Contain("Account Active");
}
