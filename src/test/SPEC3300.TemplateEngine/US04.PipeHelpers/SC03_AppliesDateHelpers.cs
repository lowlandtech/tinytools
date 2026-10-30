using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("04", "Template engine applies date pipe helpers")]
public sealed class SC03_AppliesDateHelpers : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context = new();
    private string _template = string.Empty;
    private string _result = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context holding an order date with a time and a birth date")]
    protected override void Given()
    {
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

    [When("the date helper template is rendered once")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    public SC03_AppliesDateHelpers() => ArrangeAndAct();

    [Then("the render should contain ISO Date: 2024-06-15")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_FormatsAsIsoDate() => _result.Should().Contain("ISO Date: 2024-06-15");

    [Then("the render should contain US Date: 06/15/2024")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_FormatsAsUsDate() => _result.Should().Contain("US Date: 06/15/2024");

    [Then("the render should contain Time: 14:30")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_FormatsTime() => _result.Should().Contain("Time: 14:30");

    [Then("the render should contain Full: June 15, 2024")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_FormatsFullDate() => _result.Should().Contain("Full: June 15, 2024");

    [Then("the render should contain Default Date: 1990-03-25")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_FormatsDefaultDate() => _result.Should().Contain("Default Date: 1990-03-25");

    [Then("the render should contain Custom Date: 25-Mar-1990")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_FormatsCustomDate() => _result.Should().Contain("Custom Date: 25-Mar-1990");
}
