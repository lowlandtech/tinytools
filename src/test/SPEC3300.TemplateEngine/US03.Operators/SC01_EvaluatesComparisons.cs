using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US03.Operators;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Template engine evaluates comparison operators")]
public sealed class SC01_EvaluatesComparisons : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ComparisonBoundaryUseCase>
{
    private readonly UseCases.ComparisonBoundaryUseCase useCase;
    private ExecutionContext _context = null!;
    private string _template = null!;

    public SC01_EvaluatesComparisons(UseCases.ComparisonBoundaryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context with an age, a balance and an item count")]
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

    [When("the comparison template is rendered once")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Rendered"] = Sut.Render(_template, _context);
    }

    [Then("the render should contain You can purchase alcohol.")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_EvaluatesGreaterThanOrEqual() => ((string)useCase.Cases["Rendered"]!).Should().Contain("You can purchase alcohol.");

    [Then("the render should contain Not yet eligible for senior discount.")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_EvaluatesLessThan() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Not yet eligible for senior discount.");

    [Then("the render should contain Low balance warning.")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_EvaluatesLessThanOrEqual() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Low balance warning.");

    [Then("the render should contain Bulk discount applied!")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_EvaluatesGreaterThan() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Bulk discount applied!");
}
