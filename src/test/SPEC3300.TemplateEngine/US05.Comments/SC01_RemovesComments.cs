namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US05.Comments;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("05", "Template engine removes comment blocks")]
public class WhenRenderingWithCommentsTest : WhenTestingFor<TinyTemplateEngine>
{
    private ToolContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    protected override void Given()
    {
        _context = new ToolContext();
        _context.Set("Name", "Alice");

        _template = """
            @* This is a single-line comment *@
            Hello, ${Context.Name}!
            @* 
            This is a 
            multi-line comment 
            *@
            Welcome to our platform.
            @* TODO: Add more content here *@
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldRemoveSingleLineComments()
    {
        _result.Should().NotContain("This is a single-line comment");
    }

    [Fact]
    public void ItShouldRemoveMultiLineComments()
    {
        _result.Should().NotContain("multi-line comment");
    }

    [Fact]
    public void ItShouldRemoveTodoComments()
    {
        _result.Should().NotContain("TODO:");
    }

    [Fact]
    public void ItShouldKeepActualContent()
    {
        _result.Should().Contain("Hello, Alice!");
        _result.Should().Contain("Welcome to our platform.");
    }
}
