using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC01_HandlesNestedControlFlow : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context01 = new();
    private readonly ExecutionContext _context02 = new();
    private readonly ExecutionContext _context03 = new();

    public SC01_HandlesNestedControlFlow(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("contexts that drive a nested foreach, a nested if and deep nesting")]
    protected override void Given()
    {
        _context01.Set("ShowList", true);
        _context01.Set("Items", new[] { "A", "B", "C" });

        _context02.Set("Items", new[]
        {
            new { Name = "Item1", Active = true },
            new { Name = "Item2", Active = false },
            new { Name = "Item3", Active = true }
        });

        _context03.Set("ShowAll", true);
        _context03.Set("Categories", new[]
        {
            new
            {
                Name = "Category1",
                Items = new[]
                {
                    new { Name = "SubA", Visible = true },
                    new { Name = "SubB", Visible = false }
                }
            }
        });
    }

    [When("each nested template is rendered once")]
    protected override void When()
    {
        base.When();

        useCase.Cases["NestedForeach"] = Sut.Render("""
            @if (Context.ShowList) {
            List:
            @foreach (var item in Context.Items) {
            - ${item}
            }
            }
            """, _context01);

        useCase.Cases["NestedIf"] = Sut.Render("""
            @foreach (var item in Context.Items) {
            @if (item.Active) {
            Active: ${item.Name}
            }
            }
            """, _context02);

        useCase.Cases["DeepNested"] = Sut.Render("""
            @if (Context.ShowAll) {
            @foreach (var cat in Context.Categories) {
            Category: ${cat.Name}
            @foreach (var item in cat.Items) {
            @if (item.Visible) {
              - ${item.Name}
            }
            }
            }
            }
            """, _context03);
    }

    [Then("the nested render should contain - A")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_HandlesNestedForeachInsideIf() => ((string)useCase.Cases["NestedForeach"]!).Should().Contain("List:").And.Contain("- A").And.Contain("- B").And.Contain("- C");

    [Then("the render should contain Active: Item1")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HandlesNestedIfInsideForeach() => ((string)useCase.Cases["NestedIf"]!).Should().Contain("Active: Item1").And.NotContain("Item2").And.Contain("Active: Item3");

    [Then("the deep render should contain Category: Category1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HandlesDeepNestedControlFlow() => ((string)useCase.Cases["DeepNested"]!).Should().Contain("Category: Category1").And.Contain("- SubA").And.NotContain("SubB");
}
