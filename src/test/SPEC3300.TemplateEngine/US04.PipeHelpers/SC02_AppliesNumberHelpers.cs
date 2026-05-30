using System.Globalization;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "02")]
[UserStory("04", "Template engine applies number pipe helpers")]
public class WhenRenderingWithNumberHelpersTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ToolContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        // Set invariant culture for consistent test results
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        _context = new ToolContext();
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Format Number")]
    [Fact]
    public void ItShouldFormatNumber()
    {
        ArrangeAndAct();
        _result.Should().Contain("Formatted Number: 1,500");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Format With Two Decimals")]
    [Fact]
    public void ItShouldFormatWithTwoDecimals()
    {
        ArrangeAndAct();
        _result.Should().Contain("Two Decimals: 1,234.57");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Format As Percentage")]
    [Fact]
    public void ItShouldFormatAsPercentage()
    {
        ArrangeAndAct();
        _result.Should().Contain("Percentage: 86");
        _result.Should().Contain("%");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Round To Two Decimals")]
    [Fact]
    public void ItShouldRoundToTwoDecimals()
    {
        ArrangeAndAct();
        _result.Should().Contain("Round 2: 3.14");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Apply Floor")]
    [Fact]
    public void ItShouldApplyFloor()
    {
        ArrangeAndAct();
        _result.Should().Contain("Floor: 1234");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Apply Ceiling")]
    [Fact]
    public void ItShouldApplyCeiling()
    {
        ArrangeAndAct();
        _result.Should().Contain("Ceiling: 1235");
    }
}
