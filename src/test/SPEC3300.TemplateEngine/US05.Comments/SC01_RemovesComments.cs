using ExecutionContext = LowlandTech.TinyTools.Core.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US05.Comments;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("05", "Template engine removes comment blocks")]
public class WhenRenderingWithCommentsTest : TinyToolsScenario<TinyTemplateEngine>
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Remove Single Line Comments")]
    [Fact]
    public void ItShouldRemoveSingleLineComments()
    {
        ArrangeAndAct();
        _result.Should().NotContain("This is a single-line comment");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Remove Multi Line Comments")]
    [Fact]
    public void ItShouldRemoveMultiLineComments()
    {
        ArrangeAndAct();
        _result.Should().NotContain("multi-line comment");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Remove Todo Comments")]
    [Fact]
    public void ItShouldRemoveTodoComments()
    {
        ArrangeAndAct();
        _result.Should().NotContain("TODO:");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Keep Actual Content")]
    [Fact]
    public void ItShouldKeepActualContent()
    {
        ArrangeAndAct();
        _result.Should().Contain("Hello, Alice!");
        _result.Should().Contain("Welcome to our platform.");
    }
}
