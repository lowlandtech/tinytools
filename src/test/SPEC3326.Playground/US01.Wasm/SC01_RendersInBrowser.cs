using LowlandTech.TinyTools.Playground;

namespace LowlandTech.TinyTools.Tests.SPEC3326.Playground.US01.Wasm;

[Trait(Spec.SPEC, "3326")]
[Trait(Spec.SC, "01")]
[UserStory("01", "A visitor renders a template in the browser playground")]
public sealed class SC01_RendersInBrowser : IClassFixture<UseCases.PlaygroundUseCase>
{
    private readonly UseCases.PlaygroundUseCase useCase;
    private readonly PlaygroundSession session = new();

    public SC01_RendersInBrowser(UseCases.PlaygroundUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a playground session with the default template and model")]
    private void Given()
    {
    }

    [When("the session renders the default, an edited, a broken and a malformed-model input")]
    private void When()
    {
        session.Render();
        useCase.Cases["Default"] = session.Output;

        session.Template = "Hi ${Context.Model.Name | upper}, you have ${Context.Model.Count} item(s).";
        session.Model = "{\"Name\":\"ada\",\"Count\":3}";
        session.Render();
        useCase.Cases["Edited"] = session.Output;
        useCase.Cases["EditedCode"] = session.DiagnosticCode ?? "none";

        session.Template = "@else";
        session.Render();
        useCase.Cases["Broken"] = session.Output;
        useCase.Cases["BrokenCode"] = session.DiagnosticCode ?? "none";

        session.Template = "Hello ${Context.Model.Name}";
        session.Model = "{ not json";
        session.Render();
        useCase.Cases["BadModelCode"] = session.DiagnosticCode ?? "none";

        session.Model = "{\"Name\":\"Ada\"}";
        session.Render();
        useCase.Cases["Snippets"] = $"{session.CSharpSnippet}|{session.WorkflowSnippet}|{session.CommandSnippet}";
        useCase.Cases["OutputLength"] = session.OutputLength;
    }

    [Then("the default output should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheDefaultTemplate() => ((string)useCase.Cases["Default"]!).Should().Be("Hello Ada!");

    [Then("the edited output should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersAnEditedTemplate() => ((string)useCase.Cases["Edited"]!).Should().Be("Hi ADA, you have 3 item(s).");

    [Then("the edited diagnostic should be none")]
    [Trait("UAC", "08"), Fact]
    public void UAC03b_ReportsNoDiagnosticWhenValid() => ((string)useCase.Cases["EditedCode"]!).Should().Be("none");

    [Then("the broken code should be TS007")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ReportsTheSyntaxDiagnostic() => ((string)useCase.Cases["BrokenCode"]!).Should().Be("TS007");

    [Then("the broken output should contain the position")]
    [Trait("UAC", "09"), Fact]
    public void UAC04b_ShowsLineAndColumn() => ((string)useCase.Cases["Broken"]!).Should().Contain("line 1");

    [Then("the bad model code should be TR004")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RejectsAMalformedModel() => ((string)useCase.Cases["BadModelCode"]!).Should().Be("TR004");

    [Then("the snippets should contain the engine")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ProducesCopyableSnippets() => ((string)useCase.Cases["Snippets"]!).Should().Contain("TinyTemplateEngine").And.Contain("type: template").And.Contain("tinytools render");

    [Then("the output length should be 9")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ReportsTheRenderedLength() => ((int)useCase.Cases["OutputLength"]!).Should().Be(9);
}
