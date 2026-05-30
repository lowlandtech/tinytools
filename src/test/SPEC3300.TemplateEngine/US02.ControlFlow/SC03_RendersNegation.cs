namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("02", "Template engine handles negation operators")]
public class WhenRenderingWithNegationTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ToolContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _context = new ToolContext();
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Show Inactive Message")]
    [Fact]
    public void ItShouldShowInactiveMessage()
    {
        ArrangeAndAct();
        _result.Should().Contain("Account is inactive.");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Not Show No Subscription Message")]
    [Fact]
    public void ItShouldNotShowNoSubscriptionMessage()
    {
        ArrangeAndAct();
        _result.Should().NotContain("No subscription found.");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Show No Items Message")]
    [Fact]
    public void ItShouldShowNoItemsMessage()
    {
        ArrangeAndAct();
        _result.Should().Contain("No items in cart.");
    }
}
