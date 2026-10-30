using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US03.Operators;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template engine evaluates comparison operators")]
[Story(typeof(ComparisonOperatorStory))]
public class WhenRenderingWithComparisonOperatorsTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private ComparisonOperatorResult _result = null!;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("a model whose values sit inside four comparison rules")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Age", 25);
        _context.Set("Balance", 100);
        _context.Set("ItemCount", 5);

        _template = """
            @if (Context.Age >= 21) {
            You can purchase alcohol.
            }
            @if (Context.Age < 65) {
            Not yet eligible for senior discount.
            }
            @if (Context.Balance <= 100) {
            Low balance warning.
            }
            @if (Context.ItemCount > 3) {
            Bulk discount applied!
            }
            """;
    }

    [When("the template is rendered once through the public engine")]
    protected override void When()
    {
        base.When();
        _result = ComparisonOperatorResult.From(Sut.Render(_template, _context));
    }

    [Case<ComparisonOperatorCase>(ComparisonOperatorCase.GreaterOrEqualSatisfied)]
    [Trait(Spec.UAC, "01")]
    [Then("it Should Evaluate Greater Than Or Equal")]
    [Fact]
    public void ItShouldEvaluateGreaterThanOrEqual()
    {
        ArrangeAndAct();
        _result.GreaterOrEqualRendered.Should().BeTrue();
    }

    [Case<ComparisonOperatorCase>(ComparisonOperatorCase.LessThanSatisfied)]
    [Trait(Spec.UAC, "02")]
    [Then("it Should Evaluate Less Than")]
    [Fact]
    public void ItShouldEvaluateLessThan()
    {
        ArrangeAndAct();
        _result.LessThanRendered.Should().BeTrue();
    }

    [Case<ComparisonOperatorCase>(ComparisonOperatorCase.LessOrEqualSatisfied)]
    [Trait(Spec.UAC, "03")]
    [Then("it Should Evaluate Less Than Or Equal")]
    [Fact]
    public void ItShouldEvaluateLessThanOrEqual()
    {
        ArrangeAndAct();
        _result.LessOrEqualRendered.Should().BeTrue();
    }

    [Case<ComparisonOperatorCase>(ComparisonOperatorCase.GreaterThanSatisfied)]
    [Trait(Spec.UAC, "04")]
    [Then("it Should Evaluate Greater Than")]
    [Fact]
    public void ItShouldEvaluateGreaterThan()
    {
        ArrangeAndAct();
        _result.GreaterThanRendered.Should().BeTrue();
    }
}
