using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "11")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC11_EvaluatesBooleanAndCollectionTruthiness : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context46 = new();
    private readonly ExecutionContext _context47 = new();
    private readonly ExecutionContext _context48 = new();

    public SC11_EvaluatesBooleanAndCollectionTruthiness(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["BooleanFalse"] = Sut.Render("""
            @if (Context.Flag) {
            Should not appear
            }
            Always appears
            """, _context46);

        useCase.Cases["CollectionCount"] = Sut.Render("""
            @if (Context.Items) {
            Has items
            }
            """, _context47);

        useCase.Cases["EmptyCollection"] = Sut.Render("""
            @if (Context.Items) {
            Has items
            } else {
            No items
            }
            """, _context48);
    }

    [Then("the render should not contain Should not appear")]
    [Trait("UAC", "46"), Fact]
    public void UAC46_HandlesBooleanFalse() => ((string)useCase.Cases["BooleanFalse"]!).Should().NotContain("Should not appear").And.Contain("Always appears");

    [Then("the render should contain Has items")]
    [Trait("UAC", "47"), Fact]
    public void UAC47_HandlesCollectionCountCheck() => ((string)useCase.Cases["CollectionCount"]!).Should().Contain("Has items");

    [Then("the render should contain No items")]
    [Trait("UAC", "48"), Fact]
    public void UAC48_HandlesEmptyCollectionAsFalsy() => ((string)useCase.Cases["EmptyCollection"]!).Should().Contain("No items").And.NotContain("Has items");
}
