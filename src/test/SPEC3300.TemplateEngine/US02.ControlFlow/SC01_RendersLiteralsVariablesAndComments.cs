using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template engine renders literals, variables and strips comments")]
public sealed class SC01_RendersLiteralsVariablesAndComments : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;

    public SC01_RendersLiteralsVariablesAndComments(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["Empty"] = Sut.Render("", _context);
        useCase.Cases["NullTemplate"] = Sut.Render(null!, _context);
        useCase.Cases["Plain"] = Sut.Render("Hello, World!", _context);
        useCase.Cases["Greeting"] = Sut.Render("Hello, ${Context.Name}!", _context);
        useCase.Cases["Introduction"] = Sut.Render("${Context.FirstName} ${Context.LastName} is ${Context.Age} years old.", _context);
        useCase.Cases["SingleComment"] = Sut.Render("Before @* comment *@ After", _context);
        useCase.Cases["MultiLineComment"] = Sut.Render("Before\n@* \nThis is a\nmulti-line comment\n*@\nAfter", _context);
        useCase.Cases["MultipleComments"] = Sut.Render("A @* comment1 *@ B @* comment2 *@ C", _context);
    }

    [Then("the empty template output should be empty")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersEmptyTemplate() => ((string)useCase.Cases["Empty"]!).Should().Be("");

    [Then("the null template output should be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersNullTemplate() => ((string?)useCase.Cases["NullTemplate"]).Should().BeNull();

    [Then("the literal template output should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTemplateWithoutVariables() => ((string)useCase.Cases["Plain"]!).Should().Be("Hello, World!");

    [Then("the variable template output should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersTemplateWithSimpleVariable() => ((string)useCase.Cases["Greeting"]!).Should().Be("Hello, John!");

    [Then("the multi-variable template output should be exact")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersTemplateWithMultipleVariables() => ((string)useCase.Cases["Introduction"]!).Should().Be("John Doe is 30 years old.");

    [Then("the commented template output should be exact")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RemovesSingleLineComment() => ((string)useCase.Cases["SingleComment"]!).Should().Be("Before  After");

    [Then("the multi-line comment output should contain both surrounding lines")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RemovesMultiLineComment() => ((string)useCase.Cases["MultiLineComment"]!).Should().Contain("Before").And.Contain("After").And.NotContain("multi-line comment");

    [Then("the twice commented template output should be exact")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RemovesMultipleComments() => ((string)useCase.Cases["MultipleComments"]!).Should().Be("A  B  C");
}
