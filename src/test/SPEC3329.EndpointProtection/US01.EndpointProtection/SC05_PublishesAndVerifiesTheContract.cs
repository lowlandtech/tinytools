#if NET10_0_OR_GREATER
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "05")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC05_PublishesAndVerifiesTheContract : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _host = null!;
    public SC05_PublishesAndVerifiesTheContract(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;

    [Given("a development host and the committed contract beside this scenario")]
    private void Given() => _host = new TemplateApiFactory();

    [When("the served document is compared with the committed one and with a mutated copy")]
    private async Task WhenAsync()
    {
        var served = await _host.CreateClient().GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var approved = JsonDocument.Parse(File.ReadAllText(ApprovePath())).RootElement;
        useCase.Cases["Operations"] = OpenApiContract.Operations(served);
        useCase.Cases["Missing"] = OpenApiContract.Required(OpenApiContract.Operations(served), []);
        useCase.Cases["Schemes"] = string.Join(",", OpenApiContract.Schemes(served));
        useCase.Cases["GlobalSecurity"] = served.TryGetProperty("security", out var security) && security.GetArrayLength() > 0;
        useCase.Cases["Drift"] = OpenApiContract.Differences(served, approved);

        // The gate has to refuse a contract that drifted, or "no drift" means nothing.
        var mutated = JsonNode.Parse(served.GetRawText()!)!;
        mutated["paths"]!.AsObject().Remove("/api/templates/render");
        useCase.Cases["MutatedDrift"] = OpenApiContract.Differences(
            JsonDocument.Parse(mutated.ToJsonString()).RootElement, approved);
    }

    private static string ApprovePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docs", "workitems", "README.md")))
            directory = directory.Parent;
        return Path.Combine(directory!.FullName, "src", "test", "SPEC3329.EndpointProtection",
            "US01.EndpointProtection", "openapi.approved.json");
    }

    public ValueTask InitializeAsync() { useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult()); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() { _host?.Dispose(); return ValueTask.CompletedTask; }

    [Then("the published operations should have count 7")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PublishesEveryOperation() => ((string[])useCase.Cases["Operations"]!).Should().HaveCount(7);

    [Then("the missing identifiers should have count 0")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_NamesEveryOperation() => ((string[])useCase.Cases["Missing"]!).Should().BeEmpty();

    [Then("the schemes should contain TinytoolsApiKey")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_DeclaresTheApiKeyScheme() => ((string)useCase.Cases["Schemes"]!).Split(',').Should().Contain("TinytoolsApiKey");

    [Then("the global security flag should be true")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_AppliesTheRequirementToTheDocument() => ((bool)useCase.Cases["GlobalSecurity"]!).Should().BeTrue();

    [Then("the contract drift should have count 0")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_MatchesTheCommittedContract() => ((string[])useCase.Cases["Drift"]!).Should().BeEmpty();

    [Then("the mutated contract drift should have count 1")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RefusesADriftedContract() => ((string[])useCase.Cases["MutatedDrift"]!).Should().HaveCount(1);
}
#endif
