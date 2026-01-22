namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWithStringHelpersTest : WhenTestingFor<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    protected override void Given()
    {
        _context = new ExecutionContext();
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

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldApplyUpperHelper()
    {
        _result.Should().Contain("Upper: JOHN DOE");
    }

    [Fact]
    public void ItShouldApplyLowerHelper()
    {
        _result.Should().Contain("Lower: john doe");
    }

    [Fact]
    public void ItShouldApplyCapitalizeHelper()
    {
        _result.Should().Contain("Capitalize: John doe");
    }

    [Fact]
    public void ItShouldApplyTrimHelper()
    {
        _result.Should().Contain("Trim: [Hello World]");
    }

    [Fact]
    public void ItShouldApplyTruncateHelper()
    {
        _result.Should().Contain("Truncate: This is a very long descrip...");
    }

    [Fact]
    public void ItShouldApplyReplaceHelper()
    {
        _result.Should().Contain("Replace: new/path/to/file");
    }

    [Fact]
    public void ItShouldChainHelpers()
    {
        _result.Should().Contain("Chained: JOHN DOE");
    }

    [Fact]
    public void ItShouldApplyCamelCaseHelper()
    {
        _result.Should().Contain("CamelCase: userAccountSettings");
    }

    [Fact]
    public void ItShouldApplyPascalCaseHelper()
    {
        _result.Should().Contain("PascalCase: UserAccountSettings");
    }
}

