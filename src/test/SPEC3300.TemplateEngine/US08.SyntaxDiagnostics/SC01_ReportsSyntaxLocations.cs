namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US08.SyntaxDiagnostics;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("08", "Syntax diagnostics report where a template is wrong")]
public sealed class SC01_ReportsSyntaxLocations : IClassFixture<UseCases.SyntaxDiagnosticsUseCase>
{
    private readonly UseCases.SyntaxDiagnosticsUseCase useCase;

    public SC01_ReportsSyntaxLocations(UseCases.SyntaxDiagnosticsUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("multiline templates whose malformed construct sits on a later line or column")]
    private void Given()
    {
        var engine = new TinyTemplateEngine();

        useCase.Cases["Interpolation"] = engine.ValidateSyntax("first\nsecond\nthird ${open");
        useCase.Cases["Unclosed"] = engine.ValidateSyntax("first\n  @if (Context.A == 1) {\nbody");
        useCase.Cases["StrayElse"] = engine.ValidateSyntax("first\n  @else {\nbody\n}");
        useCase.Cases["Valid"] = engine.ValidateSyntax("first\n@if (Context.A == 1) {\nmid\n}\nlast");
    }

    [When("each template is validated once through the public engine")]
    private void When()
    {
    }

    [Then("the interpolation diagnostic line should be 3")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReportsTheLaterLine() => ((TemplateSyntaxValidationResult)useCase.Cases["Interpolation"]!).Diagnostics[0].Line.Should().Be(3);

    [Then("the interpolation diagnostic column should be 7")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ReportsTheColumn() => ((TemplateSyntaxValidationResult)useCase.Cases["Interpolation"]!).Diagnostics[0].Column.Should().Be(7);

    [Then("the interpolation construct should be ${")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReportsTheConstruct() => ((TemplateSyntaxValidationResult)useCase.Cases["Interpolation"]!).Diagnostics[0].Construct.Should().Be("${");

    [Then("the unclosed diagnostic line should be 2")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ReportsTheBlockLine() => ((TemplateSyntaxValidationResult)useCase.Cases["Unclosed"]!).Diagnostics[0].Line.Should().Be(2);

    [Then("the unclosed diagnostic column should be 3")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ReportsTheBlockColumn() => ((TemplateSyntaxValidationResult)useCase.Cases["Unclosed"]!).Diagnostics[0].Column.Should().Be(3);

    [Then("the stray else line should be 2")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ReportsTheClosingConstruct() => ((TemplateSyntaxValidationResult)useCase.Cases["StrayElse"]!).Diagnostics[0].Line.Should().Be(2);

    [Then("the stray else code should be TS007")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_NamesTheClosingConstruct() => ((TemplateSyntaxValidationResult)useCase.Cases["StrayElse"]!).Diagnostics[0].Code.Should().Be("TS007");

    [Then("the valid diagnostics should be empty")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_ReportsNothingForValidTemplates() => ((TemplateSyntaxValidationResult)useCase.Cases["Valid"]!).Diagnostics.Should().BeEmpty();
}
