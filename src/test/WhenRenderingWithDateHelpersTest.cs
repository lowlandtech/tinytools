namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWithDateHelpersTest : WhenTestingFor<TinyTemplateEngine>
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
        _context.Set("OrderDate", new DateTime(2024, 6, 15, 14, 30, 0));
        _context.Set("BirthDate", new DateTime(1990, 3, 25));

        _template = """
            ISO Date: ${Context.OrderDate | format:yyyy-MM-dd}
            US Date: ${Context.OrderDate | format:MM/dd/yyyy}
            Time: ${Context.OrderDate | format:HH:mm}
            Full: ${Context.OrderDate | format:MMMM dd, yyyy}
            Default Date: ${Context.BirthDate | date}
            Custom Date: ${Context.BirthDate | date:dd-MMM-yyyy}
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldFormatAsIsoDate()
    {
        _result.Should().Contain("ISO Date: 2024-06-15");
    }

    [Fact]
    public void ItShouldFormatAsUsDate()
    {
        _result.Should().Contain("US Date: 06/15/2024");
    }

    [Fact]
    public void ItShouldFormatTime()
    {
        _result.Should().Contain("Time: 14:30");
    }

    [Fact]
    public void ItShouldFormatFullDate()
    {
        _result.Should().Contain("Full: June 15, 2024");
    }

    [Fact]
    public void ItShouldFormatDefaultDate()
    {
        _result.Should().Contain("Default Date: 1990-03-25");
    }

    [Fact]
    public void ItShouldFormatCustomDate()
    {
        _result.Should().Contain("Custom Date: 25-Mar-1990");
    }
}
