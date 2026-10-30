using LowlandTech.TinyTools.Generators;

namespace LowlandTech.TinyTools.Tests.SPEC3306.Generator.US02.BuildTime;

[Trait(Spec.SPEC, "3306")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Build-time generation produces typed renderers without runtime compilation")]
public sealed class SC01_GeneratesTypedRenderers : IClassFixture<UseCases.GeneratorUseCase>
{
    private readonly UseCases.GeneratorUseCase useCase;

    public SC01_GeneratesTypedRenderers(UseCases.GeneratorUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("one valid template, one malformed template and two colliding templates")]
    private void Given()
    {
    }

    [When("each consumer compilation runs the generator once")]
    private void When()
    {
        var valid = GeneratorHarness.Build(
            [("greeting.tmpl", "Hello ${Context.Model.Name}!", new() { ["TinyToolsModel"] = "Consumer.Model", ["TinyToolsNamespace"] = "Consumer.Templates" })],
            GeneratorHarness.ConsumerSource);
        useCase.Cases["Generated"] = valid.Generated.FirstOrDefault().Text ?? string.Empty;
        useCase.Cases["ValidDiag"] = string.Join("|", valid.Diagnostics.Select(d => d.Id + ":" + d.GetMessage()));
        useCase.Cases["GeneratedOutput"] = GeneratorHarness.Invoke(valid.Output, "Consumer.Entry", "Run");
        useCase.Cases["Errors"] = string.Join(",", valid.Output.GetDiagnostics().Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Select(d => d.Id));
        useCase.Cases["ErrorText"] = string.Join(" ~ ", valid.Output.GetDiagnostics().Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Select(d => d.GetMessage()));

        var malformed = GeneratorHarness.Build(
            [("broken.tmpl", "@if (true) {\nHello", new() { ["TinyToolsModel"] = "Consumer.Model" })],
            GeneratorHarness.ConsumerSource);
        var malformedDiagnostic = malformed.Diagnostics.FirstOrDefault();
        useCase.Cases["MalformedId"] = malformedDiagnostic?.Id ?? "none";
        useCase.Cases["MalformedPath"] = malformedDiagnostic?.Location.GetLineSpan().Path ?? "none";
        useCase.Cases["MalformedLine"] = malformedDiagnostic?.GetMessage() ?? "none";

        var collision = GeneratorHarness.Build(
            [
                ("one.tmpl", "A", new() { ["TinyToolsModel"] = "Consumer.Model", ["TinyToolsOutputName"] = "Same" }),
                ("two.tmpl", "B", new() { ["TinyToolsModel"] = "Consumer.Model", ["TinyToolsOutputName"] = "Same" }),
            ],
            GeneratorHarness.ConsumerSource);
        useCase.Cases["CollisionId"] = collision.Diagnostics.FirstOrDefault(d => d.Id == "TTG003")?.Id ?? "none";
        useCase.Cases["CollisionMessage"] = collision.Diagnostics.FirstOrDefault(d => d.Id == "TTG003")?.GetMessage() ?? "none";

        var missing = GeneratorHarness.Build([("naked.tmpl", "x", new())], GeneratorHarness.ConsumerSource);
        useCase.Cases["MissingId"] = missing.Diagnostics.FirstOrDefault()?.Id ?? "none";
    }

    [Then("the generated source should contain the model type")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DeclaresTheModelType() => ((string)useCase.Cases["Generated"]!).Should().Contain("Render(Consumer.Model model)");

    [Then("the generated source should contain no runtime compilation")]
    [Trait("UAC", "02"), Fact]
    public void UAC01b_CompilesNothingAtRuntime() => ((string)useCase.Cases["Generated"]!).Should().NotContain("Compile(").And.NotContain("Roslyn");

    [Then("the generated output should be exact")]
    [Trait("UAC", "10"), Fact]
    public void UAC03_RendersThroughTheGeneratedRenderer() => ((string)useCase.Cases["GeneratedOutput"]!).Should().Be("Hello Ada!");

    [Then("the consumer errors should be empty")]
    [Trait("UAC", "03"), Fact]
    public void UAC01c_CompilesInTheConsumer() => ((string)useCase.Cases["ErrorText"]!).Should().BeEmpty();

    [Then("the malformed identity should be TTG001")]
    [Trait("UAC", "04"), Fact]
    public void UAC02_ReportsTTG001() => ((string)useCase.Cases["MalformedId"]!).Should().Be("TTG001");

    [Then("the malformed path should be broken.tmpl")]
    [Trait("UAC", "05"), Fact]
    public void UAC02b_NamesTheSourceFile() => ((string)useCase.Cases["MalformedPath"]!).Should().EndWith("broken.tmpl");

    [Then("the malformed message should contain line 2")]
    [Trait("UAC", "06"), Fact]
    public void UAC02c_CarriesLineAndColumn() => ((string)useCase.Cases["MalformedLine"]!).Should().Contain("line 2");

    [Then("the collision identity should be TTG003")]
    [Trait("UAC", "07"), Fact]
    public void UAC04_ReportsTTG003() => ((string)useCase.Cases["CollisionId"]!).Should().Be("TTG003");

    [Then("the collision message should contain both files")]
    [Trait("UAC", "08"), Fact]
    public void UAC04b_NamesBothSources() => ((string)useCase.Cases["CollisionMessage"]!).Should().Contain("one.tmpl").And.Contain("two.tmpl");

    [Then("the missing model identity should be TTG002")]
    [Trait("UAC", "09"), Fact]
    public void UAC08_ReportsTTG002() => ((string)useCase.Cases["MissingId"]!).Should().Be("TTG002");
}
