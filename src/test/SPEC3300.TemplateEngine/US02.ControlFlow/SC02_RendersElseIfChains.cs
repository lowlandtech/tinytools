using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "02")]
[UserStory("02", "Template engine handles else-if chains")]
public sealed class SC02_RendersElseIfChains : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;
    private string _template = null!;

    public SC02_RendersElseIfChains(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context with a score and an editor role")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Score", 75);
        _context.Set("Role", "editor");

        _template = """
            Grade:
            @if (Context.Score >= 90) {
            A - Excellent!
            } else if (Context.Score >= 80) {
            B - Good job!
            } else if (Context.Score >= 70) {
            C - Satisfactory
            } else if (Context.Score >= 60) {
            D - Needs improvement
            } else {
            F - Failed
            }

            Access Level:
            @if (Context.Role == "admin") {
            Full access granted
            } else if (Context.Role == "editor") {
            Edit access granted
            } else if (Context.Role == "viewer") {
            Read-only access
            } else {
            Guest access
            }
            """;
    }

    [When("the else-if template is rendered once")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Rendered"] = Sut.Render(_template, _context);
    }

    [Then("the grade should contain C - Satisfactory")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_SelectsCorrectGrade() => ((string)useCase.Cases["Rendered"]!).Should().Contain("C - Satisfactory");

    [Then("the grade should not contain A - Excellent")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DoesNotShowOtherGrades() => ((string)useCase.Cases["Rendered"]!).Should().NotContain("A - Excellent").And.NotContain("B - Good job").And.NotContain("D - Needs improvement").And.NotContain("F - Failed");

    [Then("the access level should contain Edit access granted")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_SelectsCorrectAccessLevel() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Edit access granted");

    [Then("the access level should not contain Guest access")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DoesNotShowOtherAccessLevels() => ((string)useCase.Cases["Rendered"]!).Should().NotContain("Full access granted").And.NotContain("Read-only access").And.NotContain("Guest access");
}
