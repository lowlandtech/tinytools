using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "05")]
[UserStory("04", "Template engine applies conditional pipe helpers")]
public sealed class SC05_AppliesConditionalHelpers : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.PipeHelpersUseCase>
{
    private readonly UseCases.PipeHelpersUseCase useCase;
    private readonly ExecutionContext _context = new();
    private string _template = string.Empty;

    public SC05_AppliesConditionalHelpers(UseCases.PipeHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context holding a name, a null nickname, an empty value, booleans and a zero count")]
    protected override void Given()
    {
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

    [When("the conditional helper template is rendered once")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Rendered"] = Sut.Render(_template, _context);
    }

    [Then("the render should contain Name: Alice")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_KeepsExistingValue() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Name: Alice");

    [Then("the render should contain Nickname: No nickname")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_UsesDefaultForNull() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Nickname: No nickname");

    [Then("the render should contain Empty: N/A")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_UsesIfEmptyForEmptyString() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Empty: N/A");

    [Then("the render should contain Missing: Not set")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_UsesDefaultForMissingVariable() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Missing: Not set");

    [Then("the render should contain Active: Yes")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ShowsYesForTrue() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Active: Yes");

    [Then("the render should contain Disabled: No")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ShowsNoForFalse() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Disabled: No");

    [Then("the render should contain Custom Yes/No: Enabled")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_UsesCustomYesNoValues() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Custom Yes/No: Enabled");

    [Then("the render should contain Count Status: Empty")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_TreatsZeroAsFalsy() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Count Status: Empty");
}
