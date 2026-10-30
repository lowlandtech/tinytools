using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "05")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC05_RemovesCommentsAndSelectsBranches : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context20 = new();
    private readonly ExecutionContext _context21 = new();
    private readonly ExecutionContext _context22 = new();
    private readonly ExecutionContext _context23 = new();

    public SC05_RemovesCommentsAndSelectsBranches(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["SingleLineComment"] = Sut.Render("Hello @* this is a comment *@ ${Context.Name}!", _context20);

        useCase.Cases["MultiLineComment"] = Sut.Render("""
            Before
            @* This is a
            multi-line
            comment *@
            After: ${Context.Value}
            """, _context21);

        useCase.Cases["Grade"] = Sut.Render("""
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

        useCase.Cases["LoginBranch"] = Sut.Render("""
            @if (Context.IsLoggedIn) {
            Welcome back!
            } else {
            Please log in.
            }
            """, _context23);
    }

    [Then("the render should not contain comment")]
    [Trait("UAC", "20"), Fact]
    public void UAC20_RemovesComments() => ((string)useCase.Cases["SingleLineComment"]!).Should().Contain("Hello").And.Contain("World!").And.NotContain("comment");

    [Then("the render should contain After: Test")]
    [Trait("UAC", "21"), Fact]
    public void UAC21_RemovesMultiLineComments() => ((string)useCase.Cases["MultiLineComment"]!).Should().Contain("Before").And.Contain("After: Test").And.NotContain("multi-line");

    [Then("the render should contain Grade: C")]
    [Trait("UAC", "22"), Fact]
    public void UAC22_HandlesElseIfChain() => ((string)useCase.Cases["Grade"]!).Should().Contain("Grade: C").And.NotContain("Grade: A").And.NotContain("Grade: B").And.NotContain("Grade: F");

    [Then("the render should contain Please log in.")]
    [Trait("UAC", "23"), Fact]
    public void UAC23_HandlesElseBlock() => ((string)useCase.Cases["LoginBranch"]!).Should().Contain("Please log in.").And.NotContain("Welcome back!");
}
