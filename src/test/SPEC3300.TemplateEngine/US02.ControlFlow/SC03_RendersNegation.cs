using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("02", "Template engine handles negation operators")]
public sealed class SC03_RendersNegation : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string _result = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context with an inactive account, a subscription and an empty cart")]
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

    [When("the negation template is rendered once")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    public SC03_RendersNegation() => ArrangeAndAct();

    [Then("the render should contain Account is inactive.")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ShowsInactiveMessage() => _result.Should().Contain("Account is inactive.");

    [Then("the render should not contain No subscription found.")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DoesNotShowSubscriptionMessage() => _result.Should().NotContain("No subscription found.");

    [Then("the render should contain No items in cart.")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ShowsNoItemsMessage() => _result.Should().Contain("No items in cart.");
}
