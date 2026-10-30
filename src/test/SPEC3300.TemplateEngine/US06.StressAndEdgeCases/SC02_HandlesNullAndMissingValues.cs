using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "02")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC02_HandlesNullAndMissingValues : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context04 = new();
    private readonly ExecutionContext _context05 = new();
    private readonly ExecutionContext _context06 = new();
    private readonly ExecutionContext _context07 = new();
    private readonly ExecutionContext _context08 = new();
    private string _result04 = string.Empty;
    private string _result05 = string.Empty;
    private string _result06 = string.Empty;
    private string _result07 = string.Empty;
    private string _result08 = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with an empty collection, a null collection, a null value, no value and a name")]
    protected override void Given()
    {
        _context04.Set("Items", Array.Empty<string>());
        _context05.Set("Items", null);
        _context06.Set("Value", null);
        _context08.Set("Name", "John");
    }

    [When("each null and missing value template is rendered once")]
    protected override void When()
    {
        base.When();

        _result04 = Sut.Render("""
            Before
            @foreach (var item in Context.Items) {
            ${item}
            }
            After
            """, _context04);

        _result05 = Sut.Render("""
            Before
            @foreach (var item in Context.Items) {
            ${item}
            }
            After
            """, _context05);

        _result06 = Sut.Render("""
            @if (Context.Value == null) {
            Value is null
            }
            """, _context06);

        _result07 = Sut.Render("Value: ${Context.MissingValue ?? 'default'}", _context07);

        _result08 = Sut.Render("Hello ${Context.Name != null ? Context.Name : 'Guest'}!", _context08);
    }

    public SC02_HandlesNullAndMissingValues() => ArrangeAndAct();

    [Then("the empty collection render should contain Before")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HandlesEmptyCollection() => _result04.Should().Contain("Before").And.Contain("After");

    [Then("the null collection render should contain Before")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_HandlesNullCollectionGracefully() => _result05.Should().Contain("Before").And.Contain("After");

    [Then("the render should contain Value is null")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_HandlesNullValueInCondition() => _result06.Should().Contain("Value is null");

    [Then("the render should contain Value: default")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_HandlesMissingContextValue() => _result07.Should().Contain("Value: default");

    [Then("the greeting should be exact")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_HandlesTernaryWithNullCheck() => _result08.Should().Be("Hello John!");
}
