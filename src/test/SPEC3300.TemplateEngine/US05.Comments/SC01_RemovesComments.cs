using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US05.Comments;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("05", "Template engine removes comment blocks")]
public sealed class SC01_RemovesComments : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string _result = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a template carrying single-line, multi-line and todo comments")]
    protected override void Given()
    {
        _context = new ExecutionContext();
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

    [When("the commented template is rendered once")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    public SC01_RemovesComments() => ArrangeAndAct();

    [Then("the render should not contain This is a single-line comment")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RemovesSingleLineComments() => _result.Should().NotContain("This is a single-line comment");

    [Then("the render should not contain multi-line comment")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RemovesMultiLineComments() => _result.Should().NotContain("multi-line comment");

    [Then("the render should not contain TODO:")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RemovesTodoComments() => _result.Should().NotContain("TODO:");

    [Then("the render should contain Hello, Alice!")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_KeepsActualContent() => _result.Should().Contain("Hello, Alice!").And.Contain("Welcome to our platform.");
}
