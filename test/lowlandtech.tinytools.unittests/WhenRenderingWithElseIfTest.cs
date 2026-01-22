namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWithElseIfTest : WhenTestingFor<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

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

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldSelectCorrectGrade()
    {
        _result.Should().Contain("C - Satisfactory");
    }

    [Fact]
    public void ItShouldNotShowOtherGrades()
    {
        _result.Should().NotContain("A - Excellent");
        _result.Should().NotContain("B - Good job");
        _result.Should().NotContain("D - Needs improvement");
        _result.Should().NotContain("F - Failed");
    }

    [Fact]
    public void ItShouldSelectCorrectAccessLevel()
    {
        _result.Should().Contain("Edit access granted");
    }

    [Fact]
    public void ItShouldNotShowOtherAccessLevels()
    {
        _result.Should().NotContain("Full access granted");
        _result.Should().NotContain("Read-only access");
        _result.Should().NotContain("Guest access");
    }
}
