using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US05.Comments;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("05", "Template engine removes comment blocks")]
public sealed class SC01_RemovesComments : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.CommentsUseCase>
{
    private readonly UseCases.CommentsUseCase useCase;
    private ExecutionContext _context = null!;
    private string _template = null!;

    public SC01_RemovesComments(UseCases.CommentsUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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
        useCase.Cases["Rendered"] = Sut.Render(_template, _context);
    }

    [Then("the render should not contain This is a single-line comment")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RemovesSingleLineComments() => ((string)useCase.Cases["Rendered"]!).Should().NotContain("This is a single-line comment");

    [Then("the render should not contain multi-line comment")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RemovesMultiLineComments() => ((string)useCase.Cases["Rendered"]!).Should().NotContain("multi-line comment");

    [Then("the render should not contain TODO:")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RemovesTodoComments() => ((string)useCase.Cases["Rendered"]!).Should().NotContain("TODO:");

    [Then("the render should contain Hello, Alice!")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_KeepsActualContent() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Hello, Alice!").And.Contain("Welcome to our platform.");
}
