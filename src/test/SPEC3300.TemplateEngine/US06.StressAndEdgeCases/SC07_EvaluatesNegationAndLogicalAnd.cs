using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "07")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC07_EvaluatesNegationAndLogicalAnd : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context29 = new();
    private readonly ExecutionContext _context30 = new();
    private readonly ExecutionContext _context31 = new();
    private readonly ExecutionContext _context32 = new();

    public SC07_EvaluatesNegationAndLogicalAnd(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with an administrator, a logged in administrator, a logged in user and no items")]
    protected override void Given()
    {
        _context29.Set("IsAdmin", true);
        _context30.Set("IsLoggedIn", true);
        _context30.Set("IsAdmin", true);
        _context31.Set("IsLoggedIn", true);
        _context31.Set("IsAdmin", false);
        _context32.Set("HasItems", false);
        _context32.Set("ShowEmpty", true);
    }

    [When("each negation and conjunction template is rendered once")]
    protected override void When()
    {
        base.When();

        useCase.Cases["NegationWithParentheses"] = Sut.Render("""
            @if (!(Context.IsAdmin)) {
            Regular User
            } else {
            Admin User
            }
            """, _context29);

        useCase.Cases["LogicalAndBothTrue"] = Sut.Render("""
            @if (Context.IsLoggedIn && Context.IsAdmin) {
            Admin Panel
            }
            """, _context30);

        useCase.Cases["LogicalAndOneFalse"] = Sut.Render("""
            @if (Context.IsLoggedIn && Context.IsAdmin) {
            Admin Panel
            } else {
            Access Denied
            }
            """, _context31);

        useCase.Cases["LogicalOrOneTrue"] = Sut.Render("""
            @if (Context.HasItems || Context.ShowEmpty) {
            Show Container
            }
            """, _context32);
    }

    [Then("the render should contain Admin User")]
    [Trait("UAC", "29"), Fact]
    public void UAC29_HandlesNegationWithParentheses() => ((string)useCase.Cases["NegationWithParentheses"]!).Should().Contain("Admin User").And.NotContain("Regular User");

    [Then("the render should contain Admin Panel")]
    [Trait("UAC", "30"), Fact]
    public void UAC30_HandlesLogicalAndBothTrue() => ((string)useCase.Cases["LogicalAndBothTrue"]!).Should().Contain("Admin Panel");

    [Then("the render should contain Access Denied")]
    [Trait("UAC", "31"), Fact]
    public void UAC31_HandlesLogicalAndOneFalse() => ((string)useCase.Cases["LogicalAndOneFalse"]!).Should().Contain("Access Denied").And.NotContain("Admin Panel");

    [Then("the render should contain Show Container")]
    [Trait("UAC", "32"), Fact]
    public void UAC32_HandlesLogicalOrOneTrue() => ((string)useCase.Cases["LogicalOrOneTrue"]!).Should().Contain("Show Container");
}
