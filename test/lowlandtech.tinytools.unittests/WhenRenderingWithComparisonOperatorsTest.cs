namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWithComparisonOperatorsTest : WhenTestingFor<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

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

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldEvaluateGreaterThanOrEqual()
    {
        _result.Should().Contain("You can purchase alcohol.");
    }

    [Fact]
    public void ItShouldEvaluateLessThan()
    {
        _result.Should().Contain("Not yet eligible for senior discount.");
    }

    [Fact]
    public void ItShouldEvaluateLessThanOrEqual()
    {
        _result.Should().Contain("Low balance warning.");
    }

    [Fact]
    public void ItShouldEvaluateGreaterThan()
    {
        _result.Should().Contain("Bulk discount applied!");
    }
}
