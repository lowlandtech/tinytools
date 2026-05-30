namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("04", "Template engine applies string pipe helpers")]
public class WhenRenderingWithStringHelpersTest : TinyToolsScenario<TinyTemplateEngine>
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Apply Upper Helper")]
    [Fact]
    public void ItShouldApplyUpperHelper()
    {
        ArrangeAndAct();
        _result.Should().Contain("Upper: JOHN DOE");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Apply Lower Helper")]
    [Fact]
    public void ItShouldApplyLowerHelper()
    {
        ArrangeAndAct();
        _result.Should().Contain("Lower: john doe");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Apply Capitalize Helper")]
    [Fact]
    public void ItShouldApplyCapitalizeHelper()
    {
        ArrangeAndAct();
        _result.Should().Contain("Capitalize: John doe");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Apply Trim Helper")]
    [Fact]
    public void ItShouldApplyTrimHelper()
    {
        ArrangeAndAct();
        _result.Should().Contain("Trim: [Hello World]");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Apply Truncate Helper")]
    [Fact]
    public void ItShouldApplyTruncateHelper()
    {
        ArrangeAndAct();
        _result.Should().Contain("Truncate: This is a very long descrip...");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Apply Replace Helper")]
    [Fact]
    public void ItShouldApplyReplaceHelper()
    {
        ArrangeAndAct();
        _result.Should().Contain("Replace: new/path/to/file");
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Chain Helpers")]
    [Fact]
    public void ItShouldChainHelpers()
    {
        ArrangeAndAct();
        _result.Should().Contain("Chained: JOHN DOE");
    }

    [Trait(Spec.UAC, "08")]
    [Then("it Should Apply Camel Case Helper")]
    [Fact]
    public void ItShouldApplyCamelCaseHelper()
    {
        ArrangeAndAct();
        _result.Should().Contain("CamelCase: userAccountSettings");
    }

    [Trait(Spec.UAC, "09")]
    [Then("it Should Apply Pascal Case Helper")]
    [Fact]
    public void ItShouldApplyPascalCaseHelper()
    {
        ArrangeAndAct();
        _result.Should().Contain("PascalCase: UserAccountSettings");
    }
}

