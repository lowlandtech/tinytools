using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US02.ControlFlow;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "10")]
[UserStory("02", "Template engine renders negated conditions")]
public sealed class SC10_RendersNegatedConditions : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _simpleNegation = string.Empty;
    private string _spacedNegation = string.Empty;
    private string _truthyNegation = string.Empty;
    private string _zeroNegation = string.Empty;
    private string _emptyStringNegation = string.Empty;
    private string _nullNegation = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context that starts empty")]
    protected override void Given() => _context = new ExecutionContext();

    [When("each negated template is rendered once")]
    protected override void When()
    {
        base.When();
        _context.Set("IsActive", false);
        _simpleNegation = Sut.Render("@if(!Context.IsActive) {\nInactive\n}", _context);
        _spacedNegation = Sut.Render("@if (!Context.IsActive) {\nNot Active\n}", _context);
        _context.Set("Count", 5);
        _truthyNegation = Sut.Render("@if(!Context.Count) {\nNo Count\n} else {\nHas Count\n}", _context);
        _context.Set("Count", 0);
        _zeroNegation = Sut.Render("@if(!Context.Count) {\nZero is falsy\n}", _context);
        _context.Set("Name", "");
        _emptyStringNegation = Sut.Render("@if(!Context.Name) {\nEmpty Name\n}", _context);
        _nullNegation = Sut.Render("@if(!Context.Missing) {\nMissing is falsy\n}", _context);
    }

    public SC10_RendersNegatedConditions() => ArrangeAndAct();

    [Then("the simple negation render should contain Inactive")]
    [Trait("UAC", "50"), Fact]
    public void UAC50_HandlesSimpleNegation() => _simpleNegation.Should().Contain("Inactive");

    [Then("the spaced negation render should contain Not Active")]
    [Trait("UAC", "51"), Fact]
    public void UAC51_HandlesNegationWithoutParentheses() => _spacedNegation.Should().Contain("Not Active");

    [Then("the negated truthy render should contain Has Count")]
    [Trait("UAC", "52"), Fact]
    public void UAC52_HandlesNegationOfTruthyValue() => _truthyNegation.Should().Contain("Has Count").And.NotContain("No Count");

    [Then("the negated zero render should contain Zero is falsy")]
    [Trait("UAC", "53"), Fact]
    public void UAC53_HandlesNegationOfZero() => _zeroNegation.Should().Contain("Zero is falsy");

    [Then("the negated empty string render should contain Empty Name")]
    [Trait("UAC", "54"), Fact]
    public void UAC54_HandlesNegationOfEmptyString() => _emptyStringNegation.Should().Contain("Empty Name");

    [Then("the negated missing value render should contain Missing is falsy")]
    [Trait("UAC", "55"), Fact]
    public void UAC55_HandlesNegationOfNull() => _nullNegation.Should().Contain("Missing is falsy");
}
