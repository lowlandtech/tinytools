using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "10")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC10_PreservesWhitespaceAndInterpolatesVariables : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context41 = new();
    private readonly ExecutionContext _context42 = new();
    private readonly ExecutionContext _context43 = new();
    private readonly ExecutionContext _context44 = new();
    private readonly ExecutionContext _context45 = new();
    private string _result41 = string.Empty;
    private string _result42 = string.Empty;
    private string _result43 = string.Empty;
    private string _result44 = string.Empty;
    private string _result45 = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with a code flag, a name, three words, a prefix and a set flag")]
    protected override void Given()
    {
        _context41.Set("ShowCode", true);
        _context42.Set("Name", "Test");
        _context43.Set("First", "Hello");
        _context43.Set("Second", "World");
        _context43.Set("Third", "!");
        _context44.Set("Prefix", "pre");
        _context44.Set("Suffix", "fix");
        _context45.Set("Flag", true);
    }

    [When("each whitespace and interpolation template is rendered once")]
    protected override void When()
    {
        base.When();

        _result41 = Sut.Render("""
            @if (Context.ShowCode) {
                function test() {
                    return true;
                }
            }
            """, _context41);

        _result42 = Sut.Render("Line 1\n\nLine 3: ${Context.Name}\n\nLine 5", _context42);

        _result43 = Sut.Render("${Context.First} ${Context.Second}${Context.Third}", _context43);

        _result44 = Sut.Render("${Context.Prefix}${Context.Suffix}", _context44);

        _result45 = Sut.Render("""
            @if (Context.Flag) {
            Flag is true
            }
            """, _context45);
    }

    public SC10_PreservesWhitespaceAndInterpolatesVariables() => ArrangeAndAct();

    [Then("the render should contain the indented function")]
    [Trait("UAC", "41"), Fact]
    public void UAC41_PreservesIndentation() => _result41.Should().Contain("    function test()").And.Contain("        return true;");

    [Then("the render should contain Line 3: Test")]
    [Trait("UAC", "42"), Fact]
    public void UAC42_HandlesEmptyLines() => _result42.Should().Contain("Line 1").And.Contain("Line 3: Test").And.Contain("Line 5");

    [Then("the interpolation should be exact")]
    [Trait("UAC", "43"), Fact]
    public void UAC43_HandlesMultipleVariablesOnSameLine() => _result43.Should().Be("Hello World!");

    [Then("the joined variables should be exact")]
    [Trait("UAC", "44"), Fact]
    public void UAC44_HandlesAdjacentVariables() => _result44.Should().Be("prefix");

    [Then("the render should contain Flag is true")]
    [Trait("UAC", "45"), Fact]
    public void UAC45_HandlesBooleanTrue() => _result45.Should().Contain("Flag is true");
}
