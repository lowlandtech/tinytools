namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWithNullCoalescingTest : WhenTestingFor<TinyTemplateEngine>
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
        _context.Set("Name", "John");
        _context.Set("Nickname", null);
        _context.Set("Title", "");
        _context.Set("Company", "Acme Inc");

        _template = """
            Hello, ${Context.Name ?? "Guest"}!
            Nickname: ${Context.Nickname ?? "None provided"}
            Title: ${Context.Title ?? "No title"}
            Company: ${Context.Company ?? "Unknown"}
            Department: ${Context.Department ?? "Unassigned"}
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldUseActualValueWhenPresent()
    {
        _result.Should().Contain("Hello, John!");
    }

    [Fact]
    public void ItShouldUseDefaultWhenNull()
    {
        _result.Should().Contain("Nickname: None provided");
    }

    [Fact]
    public void ItShouldUseDefaultWhenEmpty()
    {
        _result.Should().Contain("Title: No title");
    }

    [Fact]
    public void ItShouldUseActualCompanyValue()
    {
        _result.Should().Contain("Company: Acme Inc");
    }

    [Fact]
    public void ItShouldUseDefaultWhenVariableDoesNotExist()
    {
        _result.Should().Contain("Department: Unassigned");
    }
}
