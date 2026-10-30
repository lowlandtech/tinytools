using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "08")]
[UserStory("02", "Template engine resolves variables and compares values")]
public sealed class SC08_ResolvesVariablesAndComparesValues : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.ControlFlowUseCase>
{
    private readonly UseCases.ControlFlowUseCase useCase;
    private ExecutionContext _context = null!;

    public SC08_ResolvesVariablesAndComparesValues(UseCases.ControlFlowUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts with a name")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Name", "John");
    }

    [When("each resolution, whitespace and comparison template is rendered once")]
    protected override void When()
    {
        base.When();
        useCase.Cases["ResolvedGreeting"] = Sut.ResolveVariables("Hello, ${Context.Name}!", _context);
        _context.Set("First", "John");
        _context.Set("Last", "Doe");
        useCase.Cases["ResolvedPair"] = Sut.ResolveVariables("${Context.First} ${Context.Last}", _context);
        useCase.Cases["Whitespace"] = Sut.Render("   \n\t  ", _context);
        useCase.Cases["UnmatchedBrace"] = Sut.Render("@if(Context.Test) {\nNo closing brace", _context);
        _context.Set("IntValue", 10);
        useCase.Cases["IntComparison"] = Sut.Render("@if(Context.IntValue > 5) {\nInt Greater\n}", _context);
        _context.Set("DoubleValue", 10.5);
        useCase.Cases["DoubleComparison"] = Sut.Render("@if(Context.DoubleValue >= 10.5) {\nDouble Greater or Equal\n}", _context);
        _context.Set("Name", "Alice");
        useCase.Cases["StringComparison"] = Sut.Render("@if(Context.Name == \"Alice\") {\nName is Alice\n}", _context);
        _context.Set("Name", "alice");
        useCase.Cases["CaseInsensitive"] = Sut.Render("@if(Context.Name == \"ALICE\") {\nMatch\n}", _context);
        _context.Set("Value", null);
        useCase.Cases["NullComparison"] = Sut.Render("@if(Context.Value == null) {\nIs Null\n}", _context);
        _context.Set("IsActive", true);
        useCase.Cases["BooleanComparison"] = Sut.Render("@if(Context.IsActive == true) {\nIs True\n}", _context);
    }

    [Then("the resolved greeting should be exact")]
    [Trait("UAC", "37"), Fact]
    public void UAC37_ResolvesVariablesDirectly() => ((string)useCase.Cases["ResolvedGreeting"]!).Should().Be("Hello, John!");

    [Then("the resolved name pair should be exact")]
    [Trait("UAC", "38"), Fact]
    public void UAC38_ResolvesMultipleVariables() => ((string)useCase.Cases["ResolvedPair"]!).Should().Be("John Doe");

    [Then("the whitespace render should contain the original spaces and tab")]
    [Trait("UAC", "39"), Fact]
    public void UAC39_HandlesTemplateWithOnlyWhitespace() => ((string)useCase.Cases["Whitespace"]!).Should().NotBeNull().And.Contain("   ").And.Contain("\t");

    [Then("the unmatched brace render should not be null")]
    [Trait("UAC", "40"), Fact]
    public void UAC40_HandlesUnmatchedBraces() => ((string)useCase.Cases["UnmatchedBrace"]!).Should().NotBeNull();

    [Then("the integer comparison render should contain Int Greater")]
    [Trait("UAC", "41"), Fact]
    public void UAC41_HandlesIntegerComparison() => ((string)useCase.Cases["IntComparison"]!).Should().Contain("Int Greater");

    [Then("the double comparison render should contain Double Greater or Equal")]
    [Trait("UAC", "42"), Fact]
    public void UAC42_HandlesDoubleComparison() => ((string)useCase.Cases["DoubleComparison"]!).Should().Contain("Double Greater or Equal");

    [Then("the string comparison render should contain Name is Alice")]
    [Trait("UAC", "43"), Fact]
    public void UAC43_HandlesStringComparisons() => ((string)useCase.Cases["StringComparison"]!).Should().Contain("Name is Alice");

    [Then("the case-insensitive comparison render should contain Match")]
    [Trait("UAC", "44"), Fact]
    public void UAC44_IsCaseInsensitiveForStringComparisons() => ((string)useCase.Cases["CaseInsensitive"]!).Should().Contain("Match");

    [Then("the null comparison render should contain Is Null")]
    [Trait("UAC", "45"), Fact]
    public void UAC45_HandlesNullComparisons() => ((string)useCase.Cases["NullComparison"]!).Should().Contain("Is Null");

    [Then("the boolean comparison render should contain Is True")]
    [Trait("UAC", "46"), Fact]
    public void UAC46_HandlesBooleanComparisons() => ((string)useCase.Cases["BooleanComparison"]!).Should().Contain("Is True");
}
