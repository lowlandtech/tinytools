using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "10")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC10_PreservesWhitespaceAndInterpolatesVariables : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context41 = new();
    private readonly ExecutionContext _context42 = new();
    private readonly ExecutionContext _context43 = new();
    private readonly ExecutionContext _context44 = new();
    private readonly ExecutionContext _context45 = new();

    public SC10_PreservesWhitespaceAndInterpolatesVariables(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["Indentation"] = Sut.Render("""
            @if (Context.ShowCode) {
                function test() {
                    return true;
                }
            }
            """, _context41);

        useCase.Cases["EmptyLines"] = Sut.Render("Line 1\n\nLine 3: ${Context.Name}\n\nLine 5", _context42);

        useCase.Cases["MultipleVariables"] = Sut.Render("${Context.First} ${Context.Second}${Context.Third}", _context43);

        useCase.Cases["AdjacentVariables"] = Sut.Render("${Context.Prefix}${Context.Suffix}", _context44);

        useCase.Cases["BooleanTrue"] = Sut.Render("""
            @if (Context.Flag) {
            Flag is true
            }
            """, _context45);
    }

    [Then("the render should contain the indented function")]
    [Trait("UAC", "41"), Fact]
    public void UAC41_PreservesIndentation() => ((string)useCase.Cases["Indentation"]!).Should().Contain("    function test()").And.Contain("        return true;");

    [Then("the render should contain Line 3: Test")]
    [Trait("UAC", "42"), Fact]
    public void UAC42_HandlesEmptyLines() => ((string)useCase.Cases["EmptyLines"]!).Should().Contain("Line 1").And.Contain("Line 3: Test").And.Contain("Line 5");

    [Then("the interpolation should be exact")]
    [Trait("UAC", "43"), Fact]
    public void UAC43_HandlesMultipleVariablesOnSameLine() => ((string)useCase.Cases["MultipleVariables"]!).Should().Be("Hello World!");

    [Then("the joined variables should be exact")]
    [Trait("UAC", "44"), Fact]
    public void UAC44_HandlesAdjacentVariables() => ((string)useCase.Cases["AdjacentVariables"]!).Should().Be("prefix");

    [Then("the render should contain Flag is true")]
    [Trait("UAC", "45"), Fact]
    public void UAC45_HandlesBooleanTrue() => ((string)useCase.Cases["BooleanTrue"]!).Should().Contain("Flag is true");
}
