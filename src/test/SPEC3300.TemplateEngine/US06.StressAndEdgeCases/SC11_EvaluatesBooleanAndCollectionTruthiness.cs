using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "11")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC11_EvaluatesBooleanAndCollectionTruthiness : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context46 = new();
    private readonly ExecutionContext _context47 = new();
    private readonly ExecutionContext _context48 = new();
    private string _result46 = string.Empty;
    private string _result47 = string.Empty;
    private string _result48 = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with a cleared flag, three items and an empty collection")]
    protected override void Given()
    {
        _context46.Set("Flag", false);
        _context47.Set("Items", new[] { "A", "B", "C" });
        _context48.Set("Items", Array.Empty<string>());
    }

    [When("each boolean and collection truthiness template is rendered once")]
    protected override void When()
    {
        base.When();

        _result46 = Sut.Render("""
            @if (Context.Flag) {
            Should not appear
            }
            Always appears
            """, _context46);

        _result47 = Sut.Render("""
            @if (Context.Items) {
            Has items
            }
            """, _context47);

        _result48 = Sut.Render("""
            @if (Context.Items) {
            Has items
            } else {
            No items
            }
            """, _context48);
    }

    public SC11_EvaluatesBooleanAndCollectionTruthiness() => ArrangeAndAct();

    [Then("the render should not contain Should not appear")]
    [Trait("UAC", "46"), Fact]
    public void UAC46_HandlesBooleanFalse() => _result46.Should().NotContain("Should not appear").And.Contain("Always appears");

    [Then("the render should contain Has items")]
    [Trait("UAC", "47"), Fact]
    public void UAC47_HandlesCollectionCountCheck() => _result47.Should().Contain("Has items");

    [Then("the render should contain No items")]
    [Trait("UAC", "48"), Fact]
    public void UAC48_HandlesEmptyCollectionAsFalsy() => _result48.Should().Contain("No items").And.NotContain("Has items");
}
