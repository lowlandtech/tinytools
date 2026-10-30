using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "08")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC08_EvaluatesLogicalOrAndMixedOperators : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context33 = new();
    private readonly ExecutionContext _context34 = new();
    private readonly ExecutionContext _context35 = new();
    private readonly ExecutionContext _context36 = new();

    public SC08_EvaluatesLogicalOrAndMixedOperators(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["LogicalOrBothFalse"] = Sut.Render("""
            @if (Context.HasItems || Context.ShowEmpty) {
            Show Container
            } else {
            Hidden
            }
            """, _context33);

        useCase.Cases["MixedOperators"] = Sut.Render("""
            @if (Context.IsAdmin || Context.IsModerator && Context.IsOwner) {
            Has Access
            }
            """, _context34);

        useCase.Cases["ComparisonsWithLogicalOperators"] = Sut.Render("""
            @if (Context.Age >= 18 && Context.HasLicense) {
            Can Drive
            }
            """, _context35);

        useCase.Cases["LogicalOperatorsWithNegation"] = Sut.Render("""
            @if (!Context.IsBlocked && Context.IsVerified) {
            Account Active
            }
            """, _context36);
    }

    [Then("the render should contain Hidden")]
    [Trait("UAC", "33"), Fact]
    public void UAC33_HandlesLogicalOrBothFalse() => ((string)useCase.Cases["LogicalOrBothFalse"]!).Should().Contain("Hidden").And.NotContain("Show Container");

    [Then("the render should contain Has Access")]
    [Trait("UAC", "34"), Fact]
    public void UAC34_HandlesMixedLogicalOperators() => ((string)useCase.Cases["MixedOperators"]!).Should().Contain("Has Access");

    [Then("the render should contain Can Drive")]
    [Trait("UAC", "35"), Fact]
    public void UAC35_HandlesLogicalOperatorsWithComparisons() => ((string)useCase.Cases["ComparisonsWithLogicalOperators"]!).Should().Contain("Can Drive");

    [Then("the render should contain Account Active")]
    [Trait("UAC", "36"), Fact]
    public void UAC36_HandlesLogicalOperatorsWithNegation() => ((string)useCase.Cases["LogicalOperatorsWithNegation"]!).Should().Contain("Account Active");
}
