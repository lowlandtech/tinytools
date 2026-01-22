namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWithConditionalHelpersTest : WhenTestingFor<TinyTemplateEngine>
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
        _context.Set("Name", "Alice");
        _context.Set("Nickname", null);
        _context.Set("EmptyValue", "");
        _context.Set("IsActive", true);
        _context.Set("IsDisabled", false);
        _context.Set("Count", 0);

        _template = """
            Name: ${Context.Name | default:Unknown}
            Nickname: ${Context.Nickname | default:No nickname}
            Empty: ${Context.EmptyValue | ifempty:N/A}
            Missing: ${Context.Missing | default:Not set}
            Active: ${Context.IsActive | yesno}
            Disabled: ${Context.IsDisabled | yesno}
            Custom Yes/No: ${Context.IsActive | yesno:Enabled,Disabled}
            Count Status: ${Context.Count | yesno:Has items,Empty}
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldKeepExistingValue()
    {
        _result.Should().Contain("Name: Alice");
    }

    [Fact]
    public void ItShouldUseDefaultForNull()
    {
        _result.Should().Contain("Nickname: No nickname");
    }

    [Fact]
    public void ItShouldUseIfEmptyForEmptyString()
    {
        _result.Should().Contain("Empty: N/A");
    }

    [Fact]
    public void ItShouldUseDefaultForMissingVariable()
    {
        _result.Should().Contain("Missing: Not set");
    }

    [Fact]
    public void ItShouldShowYesForTrue()
    {
        _result.Should().Contain("Active: Yes");
    }

    [Fact]
    public void ItShouldShowNoForFalse()
    {
        _result.Should().Contain("Disabled: No");
    }

    [Fact]
    public void ItShouldUseCustomYesNoValues()
    {
        _result.Should().Contain("Custom Yes/No: Enabled");
    }

    [Fact]
    public void ItShouldTreatZeroAsFalsy()
    {
        _result.Should().Contain("Count Status: Empty");
    }
}
