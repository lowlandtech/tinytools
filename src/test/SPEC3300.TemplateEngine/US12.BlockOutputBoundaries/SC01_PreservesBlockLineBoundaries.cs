namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US12.BlockOutputBoundaries;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("12", "Control-flow rendering preserves source line boundaries")]
public sealed class SC01_PreservesBlockLineBoundaries : IClassFixture<UseCases.BlockOutputUseCase>
{
    private readonly UseCases.BlockOutputUseCase useCase;

    public SC01_PreservesBlockLineBoundaries(UseCases.BlockOutputUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("multiline templates with blocks at the start, middle, end and next to blank lines")]
    private void Given()
    {
        var engine = new TinyTemplateEngine();
        var context = new ExecutionContext();
        context.Set("A", 1);

        useCase.Cases["Middle"] = engine.Render("before\n@if (Context.A == 1) {\nmid\n}\nafter", context);
        useCase.Cases["Adjacent"] = engine.Render("@if (Context.A == 1) {\none\n}\n@if (Context.A == 1) {\ntwo\n}", context);
        useCase.Cases["BlankLine"] = engine.Render("Premium.\n@if (Context.A == 2) {\n}\n\nItems:", context);
        useCase.Cases["Lf"] = engine.Render("one\n@if (Context.A == 1) {\ntwo\n}\nthree", context);
        useCase.Cases["CrLf"] = engine.Render("one\r\n@if (Context.A == 1) {\r\ntwo\r\n}\r\nthree", context);
        useCase.Cases["Nested"] = engine.Render("start\n@if (Context.A == 1) {\nouter\n@foreach (var item in Context.Items) {\n- ${item}\n}\nend\n}\nfinish", Nested());
    }

    [When("each template renders once through the public engine")]
    private void When()
    {
    }

    private static ExecutionContext Nested()
    {
        var context = new ExecutionContext();
        context.Set("A", 1);
        context.Set("Items", new[] { "a", "b" });
        return context;
    }

    [Then("the middle block output should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PreservesBoundaryAfterBlock() => ((string)useCase.Cases["Middle"]!).Should().Be("before\nmid\nafter");

    [Then("the adjacent block output should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_SeparatesAdjacentBlocks() => ((string)useCase.Cases["Adjacent"]!).Should().Be("one\ntwo");

    [Then("the blank line output should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_PreservesBlankLineAfterBlock() => ((string)useCase.Cases["BlankLine"]!).Should().Be("Premium.\n\nItems:");

    [Then("the newline normalized outputs should be equal")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersIdenticalOutputForBothNewlines() => ((string)useCase.Cases["CrLf"]!).Should().Be((string)useCase.Cases["Lf"]!);

    [Then("the nested block output should be exact")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_PreservesNestedBlockBoundaries() => ((string)useCase.Cases["Nested"]!).Should().Be("start\nouter\n- a\n- b\nend\nfinish");
}
