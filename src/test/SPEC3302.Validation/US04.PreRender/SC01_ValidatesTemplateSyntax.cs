namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US04.PreRender;

[Trait("SPEC", "3302"), Trait("US", "04"), Trait("SC", "01")]
[UserStory("04", "Engine syntax validation is an explicit operation before rendering")]
public sealed class SC01_ValidatesTemplateSyntax : IClassFixture<UseCases.PreRenderValidationUseCase>
{
    private readonly UseCases.PreRenderValidationUseCase useCase;
    private int helperCalls;

    public SC01_ValidatesTemplateSyntax(UseCases.PreRenderValidationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("an engine with a counting helper, a supported template and four malformed templates")]
    private void Given()
    {
        var engine = new TinyTemplateEngine();
        TemplateHelpers.Register("scratchcount", (value, _) =>
        {
            helperCalls++;
            return value;
        });

        var context = new ExecutionContext();
        context.Set("Name", "Ada");
        context.Set("Items", new[] { "a", "b" });

        useCase.Cases["Valid"] = engine.ValidateSyntax("Hello ${Name}\n@if (Name == \"Ada\") {\nhi\n}\n@foreach (var item in Items) {\n- ${item}\n}");
        useCase.Cases["Rendered"] = engine.Render("Hello ${Name}", context);
        useCase.Cases["UnterminatedInterpolation"] = engine.ValidateSyntax("Hello ${Name");
        useCase.Cases["UnterminatedComment"] = engine.ValidateSyntax("Hello @* never closed");
        useCase.Cases["MissingCondition"] = engine.ValidateSyntax("@if {\nhi\n}");
        useCase.Cases["UnclosedBlock"] = engine.ValidateSyntax("text\n@if (Name == \"Ada\") {\nhi");
        useCase.Cases["HelperTemplate"] = engine.ValidateSyntax("${Name | scratchcount}");
        useCase.Cases["HelperCalls"] = helperCalls;
        useCase.Cases["ExampleResultType"] = new CSharpClassTemplate().ValidateDetailed()!.GetType().Name;
    }

    [When("each template is validated once through the public engine")]
    private void When()
    {
        // The counting helper only exists to prove validation never runs it, so the
        // shared registry is left exactly as this scenario found it. Unregister is the
        // supported way to undo a process-wide registration now that the registry is
        // read-only, and per-render behavior belongs in ExecutionContext.RegisterService.
        TemplateHelpers.Unregister("scratchcount");
    }

    [Then("the valid template result should be true")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_AcceptsSupportedSyntax() => ((TemplateSyntaxValidationResult)useCase.Cases["Valid"]!).IsValid.Should().BeTrue();

    [Then("the render output should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RenderOutputIsUnchanged() => ((string)useCase.Cases["Rendered"]!).Should().Be("Hello Ada");

    [Then("the interpolation codes should have count 1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReportsUnterminatedInterpolation() => ((TemplateSyntaxValidationResult)useCase.Cases["UnterminatedInterpolation"]!).Codes.Should().Equal("TS001");

    [Then("the comment codes should have count 1")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ReportsUnterminatedComment() => ((TemplateSyntaxValidationResult)useCase.Cases["UnterminatedComment"]!).Codes.Should().Equal("TS002");

    [Then("the condition codes should have count 1")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ReportsMissingCondition() => ((TemplateSyntaxValidationResult)useCase.Cases["MissingCondition"]!).Codes.Should().Equal("TS003");

    [Then("the block codes should have count 1")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ReportsUnclosedBlock() => ((TemplateSyntaxValidationResult)useCase.Cases["UnclosedBlock"]!).Codes.Should().Equal("TS005");

    [Then("the helper template result should be true")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_DoesNotInvokeHelpers() => ((TemplateSyntaxValidationResult)useCase.Cases["HelperTemplate"]!).IsValid.Should().BeTrue();

    [Then("the helper calls should have count 0")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_LeavesHelpersUntouched() => ((int)useCase.Cases["HelperCalls"]!).Should().Be(0);

    [Then("the example result type should be TemplateValidationResult")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_KeepsTheExampleResultDistinct() => ((string)useCase.Cases["ExampleResultType"]!).Should().Be("TemplateValidationResult");
}
