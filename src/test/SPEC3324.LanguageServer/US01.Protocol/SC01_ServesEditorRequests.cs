namespace LowlandTech.TinyTools.Tests.SPEC3324.LanguageServer.US01.Protocol;

[Trait(Spec.SPEC, "3324")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Editors get diagnostics, completion and preview from one protocol server")]
public sealed class SC01_ServesEditorRequests : IClassFixture<UseCases.LanguageServerUseCase>
{
    private readonly UseCases.LanguageServerUseCase useCase;

    public SC01_ServesEditorRequests(UseCases.LanguageServerUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a buffered loopback transport and a model declared through initialization options")]
    private void Given()
    {
    }

    [When("the client initializes, opens a broken document, completes twice and requests a preview")]
    private void When()
    {
        var (initialize, diagnostics, members, helpers, preview) = UseCases.ServerHarness.Exchange();

        useCase.Cases["Capabilities"] = initialize["result"]?["capabilities"]?.ToJsonString() ?? "none";
        useCase.Cases["DiagnosticCode"] = diagnostics["params"]?["diagnostics"]?[0]?["code"]?.GetValue<string>() ?? "none";
        useCase.Cases["DiagnosticLine"] = diagnostics["params"]?["diagnostics"]?[0]?["range"]?["start"]?["line"]?.GetValue<int>() ?? -1;
        useCase.Cases["Members"] = members["result"]?.ToJsonString() ?? "none";
        useCase.Cases["Helpers"] = helpers["result"]?.ToJsonString() ?? "none";
        useCase.Cases["PreviewText"] = preview["result"]?["text"]?.GetValue<string>() ?? "none";
        useCase.Cases["PreviewRevision"] = preview["result"]?["revision"]?.GetValue<int>() ?? -1;
    }

    [Then("the capabilities should contain the preview")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_AdvertisesCapabilities() => ((string)useCase.Cases["Capabilities"]!).Should().Contain("tinytools.preview");

    [Then("the diagnostic code should be TS007")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_PublishesTheSyntaxDiagnostic() => ((string)useCase.Cases["DiagnosticCode"]!).Should().Be("TS007");

    [Then("the diagnostic line should be 1")]
    [Trait("UAC", "06"), Fact]
    public void UAC02b_ReportsTheZeroBasedLine() => ((int)useCase.Cases["DiagnosticLine"]!).Should().Be(1);

    [Then("the members should contain Name and Address")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_CompletesModelMembers() => ((string)useCase.Cases["Members"]!).Should().Contain("\"Name\"").And.Contain("\"Address\"");

    [Then("the helpers should contain upper")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_CompletesHelperNames() => ((string)useCase.Cases["Helpers"]!).Should().Contain("\"upper\"");

    [Then("the preview text should be exact")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersThePreview() => ((string)useCase.Cases["PreviewText"]!).Should().Be("Hello Ada");

    [Then("the preview revision should be 0")]
    [Trait("UAC", "07"), Fact]
    public void UAC05b_ReportsTheRequestedRevision() => ((int)useCase.Cases["PreviewRevision"]!).Should().Be(0);
}
