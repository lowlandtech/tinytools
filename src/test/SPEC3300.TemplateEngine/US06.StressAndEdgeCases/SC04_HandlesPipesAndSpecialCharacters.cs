using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "04")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC04_HandlesPipesAndSpecialCharacters : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context14 = new();
    private readonly ExecutionContext _context15 = new();
    private readonly ExecutionContext _context16 = new();
    private readonly ExecutionContext _context17 = new();
    private readonly ExecutionContext _context18 = new();
    private readonly ExecutionContext _context19 = new();
    private string _result14 = string.Empty;
    private string _result15 = string.Empty;
    private string _result16 = string.Empty;
    private string _result17 = string.Empty;
    private string _result18 = string.Empty;
    private string _result19 = string.Empty;

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

        _result14 = Sut.Render("${Context.Name | trim | upper}", _context14);

        _result15 = Sut.Render("""
            @foreach (var name in Context.Names) {
            ${name | capitalize}
            }
            """, _context15);

        _result16 = Sut.Render("${Context.Text | truncate:20}", _context16);

        _result17 = Sut.Render("Code: ${Context.Code}", _context17);

        _result18 = Sut.Render("JSON: ${Context.Json}", _context18);

        _result19 = Sut.Render("Price: ${Context.Price}", _context19);
    }

    public SC04_HandlesPipesAndSpecialCharacters() => ArrangeAndAct();

    [Then("the trimmed name should be exact")]
    [Trait("UAC", "14"), Fact]
    public void UAC14_HandlesMultiplePipeHelpers() => _result14.Should().Be("JOHN DOE");

    [Then("the render should contain Alice")]
    [Trait("UAC", "15"), Fact]
    public void UAC15_HandlesPipeHelpersInForeach() => _result15.Should().Contain("Alice").And.Contain("Bob").And.Contain("Carol");

    [Then("the truncated output should be bounded")]
    [Trait("UAC", "16"), Fact]
    public void UAC16_HandlesPipeWithArguments() => _result16.Length.Should().BeLessThanOrEqualTo(23);

    [Then("the render should contain the comparison code")]
    [Trait("UAC", "17"), Fact]
    public void UAC17_HandlesSpecialCharactersInContent() => _result17.Should().Contain("if (x > 0 && y < 10)");

    [Then("the render should contain the json braces")]
    [Trait("UAC", "18"), Fact]
    public void UAC18_HandlesBracesInStringValues() => _result18.Should().Contain("{ \"key\": \"value\" }");

    [Then("the price line should be $99.99")]
    [Trait("UAC", "19"), Fact]
    public void UAC19_HandlesDollarSignInValues() => _result19.Should().Be("Price: $99.99");
}
