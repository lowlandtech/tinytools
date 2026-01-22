namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWithCollectionHelpersTest : WhenTestingFor<TinyTemplateEngine>
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
        _context.Set("Tags", new[] { "csharp", "dotnet", "templates" });
        _context.Set("Numbers", new[] { 1, 2, 3, 4, 5 });
        _context.Set("EmptyList", Array.Empty<string>());
        _context.Set("Word", "Hello");

        _template = """
            Count: ${Context.Tags | count}
            First: ${Context.Tags | first}
            Last: ${Context.Tags | last}
            Join: ${Context.Tags | join:, }
            Join Dash: ${Context.Numbers | join:-}
            Reverse String: ${Context.Word | reverse}
            Empty Count: ${Context.EmptyList | count}
            String Length: ${Context.Word | count}
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldCountItems()
    {
        _result.Should().Contain("Count: 3");
    }

    [Fact]
    public void ItShouldGetFirstItem()
    {
        _result.Should().Contain("First: csharp");
    }

    [Fact]
    public void ItShouldGetLastItem()
    {
        _result.Should().Contain("Last: templates");
    }

    [Fact]
    public void ItShouldJoinWithComma()
    {
        _result.Should().Contain("Join: csharp, dotnet, templates");
    }

    [Fact]
    public void ItShouldJoinWithDash()
    {
        _result.Should().Contain("Join Dash: 1-2-3-4-5");
    }

    [Fact]
    public void ItShouldReverseString()
    {
        _result.Should().Contain("Reverse String: olleH");
    }

    [Fact]
    public void ItShouldCountEmptyList()
    {
        _result.Should().Contain("Empty Count: 0");
    }

    [Fact]
    public void ItShouldCountStringLength()
    {
        _result.Should().Contain("String Length: 5");
    }
}
