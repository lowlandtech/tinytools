using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "04")]
[UserStory("02", "Template engine handles null coalescing")]
public sealed class SC04_RendersNullCoalescing : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;
    private string _template = null!;

    public SC04_RendersNullCoalescing(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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
        useCase.Cases["Rendered"] = Sut.Render(_template, _context);
    }

    [Then("the render should contain Hello, John!")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_UsesActualValueWhenPresent() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Hello, John!");

    [Then("the render should contain Nickname: None provided")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_UsesDefaultWhenNull() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Nickname: None provided");

    [Then("the render should contain Title: No title")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_UsesDefaultWhenEmpty() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Title: No title");

    [Then("the render should contain Company: Acme Inc")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_UsesActualCompanyValue() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Company: Acme Inc");

    [Then("the render should contain Department: Unassigned")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_UsesDefaultWhenVariableDoesNotExist() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Department: Unassigned");
}
