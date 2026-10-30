using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "04")]
[UserStory("02", "Template engine handles null coalescing")]
public sealed class SC04_RendersNullCoalescing : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string _result = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context with present, null, empty and missing values")]
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

    [When("the coalescing template is rendered once")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    public SC04_RendersNullCoalescing() => ArrangeAndAct();

    [Then("the render should contain Hello, John!")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_UsesActualValueWhenPresent() => _result.Should().Contain("Hello, John!");

    [Then("the render should contain Nickname: None provided")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_UsesDefaultWhenNull() => _result.Should().Contain("Nickname: None provided");

    [Then("the render should contain Title: No title")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_UsesDefaultWhenEmpty() => _result.Should().Contain("Title: No title");

    [Then("the render should contain Company: Acme Inc")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_UsesActualCompanyValue() => _result.Should().Contain("Company: Acme Inc");

    [Then("the render should contain Department: Unassigned")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_UsesDefaultWhenVariableDoesNotExist() => _result.Should().Contain("Department: Unassigned");
}
