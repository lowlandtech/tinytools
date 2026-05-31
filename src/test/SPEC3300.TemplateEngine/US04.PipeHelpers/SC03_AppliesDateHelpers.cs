using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("04", "Template engine applies date pipe helpers")]
public class WhenRenderingWithDateHelpersTest : TinyToolsScenario<TinyTemplateEngine>
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Format As Iso Date")]
    [Fact]
    public void ItShouldFormatAsIsoDate()
    {
        ArrangeAndAct();
        _result.Should().Contain("ISO Date: 2024-06-15");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Format As Us Date")]
    [Fact]
    public void ItShouldFormatAsUsDate()
    {
        ArrangeAndAct();
        _result.Should().Contain("US Date: 06/15/2024");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Format Time")]
    [Fact]
    public void ItShouldFormatTime()
    {
        ArrangeAndAct();
        _result.Should().Contain("Time: 14:30");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Format Full Date")]
    [Fact]
    public void ItShouldFormatFullDate()
    {
        ArrangeAndAct();
        _result.Should().Contain("Full: June 15, 2024");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Format Default Date")]
    [Fact]
    public void ItShouldFormatDefaultDate()
    {
        ArrangeAndAct();
        _result.Should().Contain("Default Date: 1990-03-25");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Format Custom Date")]
    [Fact]
    public void ItShouldFormatCustomDate()
    {
        ArrangeAndAct();
        _result.Should().Contain("Custom Date: 25-Mar-1990");
    }
}
