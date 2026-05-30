namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "04")]
[UserStory("02", "Template engine handles null coalescing")]
public class WhenRenderingWithNullCoalescingTest : TinyToolsScenario<TinyTemplateEngine>
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
        _context = new ToolContext();
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Use Actual Value When Present")]
    [Fact]
    public void ItShouldUseActualValueWhenPresent()
    {
        ArrangeAndAct();
        _result.Should().Contain("Hello, John!");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Use Default When Null")]
    [Fact]
    public void ItShouldUseDefaultWhenNull()
    {
        ArrangeAndAct();
        _result.Should().Contain("Nickname: None provided");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Use Default When Empty")]
    [Fact]
    public void ItShouldUseDefaultWhenEmpty()
    {
        ArrangeAndAct();
        _result.Should().Contain("Title: No title");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Use Actual Company Value")]
    [Fact]
    public void ItShouldUseActualCompanyValue()
    {
        ArrangeAndAct();
        _result.Should().Contain("Company: Acme Inc");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Use Default When Variable Does Not Exist")]
    [Fact]
    public void ItShouldUseDefaultWhenVariableDoesNotExist()
    {
        ArrangeAndAct();
        _result.Should().Contain("Department: Unassigned");
    }
}
