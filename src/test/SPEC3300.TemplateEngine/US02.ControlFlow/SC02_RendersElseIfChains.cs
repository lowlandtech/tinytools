using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "02")]
[UserStory("02", "Template engine handles else-if chains")]
public class WhenRenderingWithElseIfTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Select Correct Grade")]
    [Fact]
    public void ItShouldSelectCorrectGrade()
    {
        ArrangeAndAct();
        _result.Should().Contain("C - Satisfactory");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Not Show Other Grades")]
    [Fact]
    public void ItShouldNotShowOtherGrades()
    {
        ArrangeAndAct();
        _result.Should().NotContain("A - Excellent");
        _result.Should().NotContain("B - Good job");
        _result.Should().NotContain("D - Needs improvement");
        _result.Should().NotContain("F - Failed");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Select Correct Access Level")]
    [Fact]
    public void ItShouldSelectCorrectAccessLevel()
    {
        ArrangeAndAct();
        _result.Should().Contain("Edit access granted");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Not Show Other Access Levels")]
    [Fact]
    public void ItShouldNotShowOtherAccessLevels()
    {
        ArrangeAndAct();
        _result.Should().NotContain("Full access granted");
        _result.Should().NotContain("Read-only access");
        _result.Should().NotContain("Guest access");
    }
}
