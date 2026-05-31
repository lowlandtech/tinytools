using ExecutionContext = LowlandTech.TinyTools.Core.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US03.Operators;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template engine evaluates comparison operators")]
public class WhenRenderingWithComparisonOperatorsTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Evaluate Greater Than Or Equal")]
    [Fact]
    public void ItShouldEvaluateGreaterThanOrEqual()
    {
        ArrangeAndAct();
        _result.Should().Contain("You can purchase alcohol.");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Evaluate Less Than")]
    [Fact]
    public void ItShouldEvaluateLessThan()
    {
        ArrangeAndAct();
        _result.Should().Contain("Not yet eligible for senior discount.");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Evaluate Less Than Or Equal")]
    [Fact]
    public void ItShouldEvaluateLessThanOrEqual()
    {
        ArrangeAndAct();
        _result.Should().Contain("Low balance warning.");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Evaluate Greater Than")]
    [Fact]
    public void ItShouldEvaluateGreaterThan()
    {
        ArrangeAndAct();
        _result.Should().Contain("Bulk discount applied!");
    }
}
