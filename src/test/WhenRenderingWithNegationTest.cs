namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWithNegationTest : WhenTestingFor<TinyTemplateEngine>
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
        _context.Set("IsActive", false);
        _context.Set("HasSubscription", true);
        _context.Set("Count", 0);

        _template = """
            @if (!Context.IsActive) {
            Account is inactive.
            }
            @if (!Context.HasSubscription) {
            No subscription found.
            }
            @if (!(Context.Count > 0)) {
            No items in cart.
            }
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldShowInactiveMessage()
    {
        _result.Should().Contain("Account is inactive.");
    }

    [Fact]
    public void ItShouldNotShowNoSubscriptionMessage()
    {
        _result.Should().NotContain("No subscription found.");
    }

    [Fact]
    public void ItShouldShowNoItemsMessage()
    {
        _result.Should().Contain("No items in cart.");
    }
}
