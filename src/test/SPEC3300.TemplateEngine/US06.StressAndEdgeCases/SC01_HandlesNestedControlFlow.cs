using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC01_HandlesNestedControlFlow : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context01 = new();
    private readonly ExecutionContext _context02 = new();
    private readonly ExecutionContext _context03 = new();
    private string _result01 = string.Empty;
    private string _result02 = string.Empty;
    private string _result03 = string.Empty;

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

        _result01 = Sut.Render("""
            @if (Context.ShowList) {
            List:
            @foreach (var item in Context.Items) {
            - ${item}
            }
            }
            """, _context01);

        _result02 = Sut.Render("""
            @foreach (var item in Context.Items) {
            @if (item.Active) {
            Active: ${item.Name}
            }
            }
            """, _context02);

        _result03 = Sut.Render("""
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

    public SC01_HandlesNestedControlFlow() => ArrangeAndAct();

    [Then("the nested render should contain - A")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_HandlesNestedForeachInsideIf() => _result01.Should().Contain("List:").And.Contain("- A").And.Contain("- B").And.Contain("- C");

    [Then("the render should contain Active: Item1")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HandlesNestedIfInsideForeach() => _result02.Should().Contain("Active: Item1").And.NotContain("Item2").And.Contain("Active: Item3");

    [Then("the deep render should contain Category: Category1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HandlesDeepNestedControlFlow() => _result03.Should().Contain("Category: Category1").And.Contain("- SubA").And.NotContain("SubB");
}
