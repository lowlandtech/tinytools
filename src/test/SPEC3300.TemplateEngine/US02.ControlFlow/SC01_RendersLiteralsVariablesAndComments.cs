using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template engine renders literals, variables and strips comments")]
public sealed class SC01_RendersLiteralsVariablesAndComments : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _empty = string.Empty;
    private string? _nullTemplate;
    private string _plain = string.Empty;
    private string _greeting = string.Empty;
    private string _introduction = string.Empty;
    private string _singleComment = string.Empty;
    private string _multiLineComment = string.Empty;
    private string _multipleComments = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context holding a name, a full name and an age")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Name", "John");
        _context.Set("FirstName", "John");
        _context.Set("LastName", "Doe");
        _context.Set("Age", 30);
    }

    [When("each literal, variable and comment template is rendered once")]
    protected override void When()
    {
        base.When();

        _empty = Sut.Render("", _context);
        _nullTemplate = Sut.Render(null!, _context);
        _plain = Sut.Render("Hello, World!", _context);
        _greeting = Sut.Render("Hello, ${Context.Name}!", _context);
        _introduction = Sut.Render("${Context.FirstName} ${Context.LastName} is ${Context.Age} years old.", _context);
        _singleComment = Sut.Render("Before @* comment *@ After", _context);
        _multiLineComment = Sut.Render("Before\n@* \nThis is a\nmulti-line comment\n*@\nAfter", _context);
        _multipleComments = Sut.Render("A @* comment1 *@ B @* comment2 *@ C", _context);
    }

    public SC01_RendersLiteralsVariablesAndComments() => ArrangeAndAct();

    [Then("the empty template output should be empty")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersEmptyTemplate() => _empty.Should().Be("");

    [Then("the null template output should be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersNullTemplate() => _nullTemplate.Should().BeNull();

    [Then("the literal template output should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTemplateWithoutVariables() => _plain.Should().Be("Hello, World!");

    [Then("the variable template output should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersTemplateWithSimpleVariable() => _greeting.Should().Be("Hello, John!");

    [Then("the multi-variable template output should be exact")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersTemplateWithMultipleVariables() => _introduction.Should().Be("John Doe is 30 years old.");

    [Then("the commented template output should be exact")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RemovesSingleLineComment() => _singleComment.Should().Be("Before  After");

    [Then("the multi-line comment output should contain both surrounding lines")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RemovesMultiLineComment() => _multiLineComment.Should().Contain("Before").And.Contain("After").And.NotContain("multi-line comment");

    [Then("the twice commented template output should be exact")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RemovesMultipleComments() => _multipleComments.Should().Be("A  B  C");
}
