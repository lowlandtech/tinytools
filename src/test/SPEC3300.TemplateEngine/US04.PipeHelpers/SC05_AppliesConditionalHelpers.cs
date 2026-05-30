namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "05")]
[UserStory("04", "Template engine applies conditional pipe helpers")]
public class WhenRenderingWithConditionalHelpersTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ToolContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _context = new ToolContext();
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Keep Existing Value")]
    [Fact]
    public void ItShouldKeepExistingValue()
    {
        ArrangeAndAct();
        _result.Should().Contain("Name: Alice");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Use Default For Null")]
    [Fact]
    public void ItShouldUseDefaultForNull()
    {
        ArrangeAndAct();
        _result.Should().Contain("Nickname: No nickname");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Use If Empty For Empty String")]
    [Fact]
    public void ItShouldUseIfEmptyForEmptyString()
    {
        ArrangeAndAct();
        _result.Should().Contain("Empty: N/A");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Use Default For Missing Variable")]
    [Fact]
    public void ItShouldUseDefaultForMissingVariable()
    {
        ArrangeAndAct();
        _result.Should().Contain("Missing: Not set");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Show Yes For True")]
    [Fact]
    public void ItShouldShowYesForTrue()
    {
        ArrangeAndAct();
        _result.Should().Contain("Active: Yes");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Show No For False")]
    [Fact]
    public void ItShouldShowNoForFalse()
    {
        ArrangeAndAct();
        _result.Should().Contain("Disabled: No");
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Use Custom Yes No Values")]
    [Fact]
    public void ItShouldUseCustomYesNoValues()
    {
        ArrangeAndAct();
        _result.Should().Contain("Custom Yes/No: Enabled");
    }

    [Trait(Spec.UAC, "08")]
    [Then("it Should Treat Zero As Falsy")]
    [Fact]
    public void ItShouldTreatZeroAsFalsy()
    {
        ArrangeAndAct();
        _result.Should().Contain("Count Status: Empty");
    }
}
