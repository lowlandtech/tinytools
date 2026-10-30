using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "04")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC04_HandlesPipesAndSpecialCharacters : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context14 = new();
    private readonly ExecutionContext _context15 = new();
    private readonly ExecutionContext _context16 = new();
    private readonly ExecutionContext _context17 = new();
    private readonly ExecutionContext _context18 = new();
    private readonly ExecutionContext _context19 = new();

    public SC04_HandlesPipesAndSpecialCharacters(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with a padded name, three names, a long text and special characters")]
    protected override void Given()
    {
        _context14.Set("Name", "  john doe  ");
        _context15.Set("Names", new[] { "alice", "bob", "carol" });
        _context16.Set("Text", "This is a very long text that should be truncated");
        _context17.Set("Code", "if (x > 0 && y < 10) { return true; }");
        _context18.Set("Json", "{ \"key\": \"value\" }");
        _context19.Set("Price", "$99.99");
    }

    [When("each pipe, escaping and special character template is rendered once")]
    protected override void When()
    {
        base.When();

        useCase.Cases["TrimmedName"] = Sut.Render("${Context.Name | trim | upper}", _context14);

        useCase.Cases["CapitalizedNames"] = Sut.Render("""
            @foreach (var name in Context.Names) {
            ${name | capitalize}
            }
            """, _context15);

        useCase.Cases["TruncatedText"] = Sut.Render("${Context.Text | truncate:20}", _context16);

        useCase.Cases["Code"] = Sut.Render("Code: ${Context.Code}", _context17);

        useCase.Cases["Json"] = Sut.Render("JSON: ${Context.Json}", _context18);

        useCase.Cases["Price"] = Sut.Render("Price: ${Context.Price}", _context19);
    }

    [Then("the trimmed name should be exact")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_HandlesMultiplePipeHelpers() => ((string)useCase.Cases["TrimmedName"]!).Should().Be("JOHN DOE");

    [Then("the render should contain Alice")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_HandlesPipeHelpersInForeach() => ((string)useCase.Cases["CapitalizedNames"]!).Should().Contain("Alice").And.Contain("Bob").And.Contain("Carol");

    [Then("the truncated output should be bounded")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_HandlesPipeWithArguments() => ((string)useCase.Cases["TruncatedText"]!).Length.Should().BeLessThanOrEqualTo(23);

    [Then("the render should contain the comparison code")]
    [Trait("UAC", "17"), Fact]
    public void UAC17_HandlesSpecialCharactersInContent() => ((string)useCase.Cases["Code"]!).Should().Contain("if (x > 0 && y < 10)");

    [Then("the render should contain the json braces")]
    [Trait("UAC", "18"), Fact]
    public void UAC18_HandlesBracesInStringValues() => ((string)useCase.Cases["Json"]!).Should().Contain("{ \"key\": \"value\" }");

    [Then("the price line should be $99.99")]
    [Trait("UAC", "19"), Fact]
    public void UAC19_HandlesDollarSignInValues() => ((string)useCase.Cases["Price"]!).Should().Be("Price: $99.99");
}
