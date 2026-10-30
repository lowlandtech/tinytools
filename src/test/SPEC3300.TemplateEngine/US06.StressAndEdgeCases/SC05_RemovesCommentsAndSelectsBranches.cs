using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "05")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC05_RemovesCommentsAndSelectsBranches : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context20 = new();
    private readonly ExecutionContext _context21 = new();
    private readonly ExecutionContext _context22 = new();
    private readonly ExecutionContext _context23 = new();
    private string _result20 = string.Empty;
    private string _result21 = string.Empty;
    private string _result22 = string.Empty;
    private string _result23 = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with a name, a value, a score and a logged out user")]
    protected override void Given()
    {
        _context20.Set("Name", "World");
        _context21.Set("Value", "Test");
        _context22.Set("Score", 75);
        _context23.Set("IsLoggedIn", false);
    }

    [When("each commented and branching template is rendered once")]
    protected override void When()
    {
        base.When();

        _result20 = Sut.Render("Hello @* this is a comment *@ ${Context.Name}!", _context20);

        _result21 = Sut.Render("""
            Before
            @* This is a
            multi-line
            comment *@
            After: ${Context.Value}
            """, _context21);

        _result22 = Sut.Render("""
            @if (Context.Score >= 90) {
            Grade: A
            } else if (Context.Score >= 80) {
            Grade: B
            } else if (Context.Score >= 70) {
            Grade: C
            } else {
            Grade: F
            }
            """, _context22);

        _result23 = Sut.Render("""
            @if (Context.IsLoggedIn) {
            Welcome back!
            } else {
            Please log in.
            }
            """, _context23);
    }

    public SC05_RemovesCommentsAndSelectsBranches() => ArrangeAndAct();

    [Then("the render should not contain comment")]
    [Trait("UAC", "20"), Fact]
    public void UAC20_RemovesComments() => _result20.Should().Contain("Hello").And.Contain("World!").And.NotContain("comment");

    [Then("the render should contain After: Test")]
    [Trait("UAC", "21"), Fact]
    public void UAC21_RemovesMultiLineComments() => _result21.Should().Contain("Before").And.Contain("After: Test").And.NotContain("multi-line");

    [Then("the render should contain Grade: C")]
    [Trait("UAC", "22"), Fact]
    public void UAC22_HandlesElseIfChain() => _result22.Should().Contain("Grade: C").And.NotContain("Grade: A").And.NotContain("Grade: B").And.NotContain("Grade: F");

    [Then("the render should contain Please log in.")]
    [Trait("UAC", "23"), Fact]
    public void UAC23_HandlesElseBlock() => _result23.Should().Contain("Please log in.").And.NotContain("Welcome back!");
}
