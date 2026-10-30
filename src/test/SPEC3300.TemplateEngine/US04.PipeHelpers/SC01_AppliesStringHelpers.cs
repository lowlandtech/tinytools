using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("04", "Template engine applies string pipe helpers")]
public sealed class SC01_AppliesStringHelpers : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context = new();
    private string _template = string.Empty;
    private string _result = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context holding a lowercase name, a padded title, a long description, a path and an underscored class name")]
    protected override void Given()
    {
        _context.Set("Name", "john doe");
        _context.Set("Title", "  Hello World  ");
        _context.Set("Description", "This is a very long description that needs to be truncated for display purposes.");
        _context.Set("Path", "old/path/to/file");
        _context.Set("ClassName", "user_account_settings");
        _context.Set("PropertyName", "firstName");

        _template = """
            Upper: ${Context.Name | upper}
            Lower: ${Context.Name | lower}
            Capitalize: ${Context.Name | capitalize}
            Trim: [${Context.Title | trim}]
            Truncate: ${Context.Description | truncate:30}
            Replace: ${Context.Path | replace:old,new}
            Chained: ${Context.Name | trim | upper}
            CamelCase: ${Context.ClassName | camelcase}
            PascalCase: ${Context.ClassName | pascalcase}
            """;
    }

    [When("the string helper template is rendered once")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    public SC01_AppliesStringHelpers() => ArrangeAndAct();

    [Then("the render should contain Upper: JOHN DOE")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_AppliesUpperHelper() => _result.Should().Contain("Upper: JOHN DOE");

    [Then("the render should contain Lower: john doe")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_AppliesLowerHelper() => _result.Should().Contain("Lower: john doe");

    [Then("the render should contain Capitalize: John doe")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_AppliesCapitalizeHelper() => _result.Should().Contain("Capitalize: John doe");

    [Then("the render should contain Trim: [Hello World]")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_AppliesTrimHelper() => _result.Should().Contain("Trim: [Hello World]");

    [Then("the render should contain Truncate: This is a very long descrip...")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_AppliesTruncateHelper() => _result.Should().Contain("Truncate: This is a very long descrip...");

    [Then("the render should contain Replace: new/path/to/file")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_AppliesReplaceHelper() => _result.Should().Contain("Replace: new/path/to/file");

    [Then("the render should contain Chained: JOHN DOE")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ChainsHelpers() => _result.Should().Contain("Chained: JOHN DOE");

    [Then("the render should contain CamelCase: userAccountSettings")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_AppliesCamelCaseHelper() => _result.Should().Contain("CamelCase: userAccountSettings");

    [Then("the render should contain PascalCase: UserAccountSettings")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_AppliesPascalCaseHelper() => _result.Should().Contain("PascalCase: UserAccountSettings");
}
