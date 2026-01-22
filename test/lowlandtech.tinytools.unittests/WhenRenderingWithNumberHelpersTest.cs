using System.Globalization;

namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWithNumberHelpersTest : WhenTestingFor<TinyTemplateEngine>
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
        // Set invariant culture for consistent test results
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        _context = new ExecutionContext();
        _context.Set("Price", 1234.567);
        _context.Set("Quantity", 1500);
        _context.Set("Percentage", 0.856);
        _context.Set("Pi", 3.14159265);

        _template = """
            Formatted Number: ${Context.Quantity | number}
            Two Decimals: ${Context.Price | format:N2}
            Percentage: ${Context.Percentage | format:P0}
            Round 2: ${Context.Pi | round:2}
            Floor: ${Context.Price | floor}
            Ceiling: ${Context.Price | ceiling}
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldFormatNumber()
    {
        _result.Should().Contain("Formatted Number: 1,500");
    }

    [Fact]
    public void ItShouldFormatWithTwoDecimals()
    {
        _result.Should().Contain("Two Decimals: 1,234.57");
    }

    [Fact]
    public void ItShouldFormatAsPercentage()
    {
        _result.Should().Contain("Percentage: 86");
        _result.Should().Contain("%");
    }

    [Fact]
    public void ItShouldRoundToTwoDecimals()
    {
        _result.Should().Contain("Round 2: 3.14");
    }

    [Fact]
    public void ItShouldApplyFloor()
    {
        _result.Should().Contain("Floor: 1234");
    }

    [Fact]
    public void ItShouldApplyCeiling()
    {
        _result.Should().Contain("Ceiling: 1235");
    }
}
