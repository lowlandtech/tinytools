#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "01")]
[UserStory("02", "The server serves the template contract over HTTP")]
public sealed class SC01_ServesTemplateContract : IAsyncLifetime, IClassFixture<UseCases.TemplateApiUseCase>
{
    private readonly UseCases.TemplateApiUseCase useCase;
    private TemplateApiFactory _development = null!;
    private TemplateApiFactory _production = null!;

    public SC01_ServesTemplateContract(UseCases.TemplateApiUseCase useCase) => this.useCase = useCase;

    [Given("a served template library in development and in production with and without a key")]
    private void Given()
    {
        _development = new TemplateApiFactory();
        _production = new TemplateApiFactory("Production", "secret-key");
    }

    [When("the contract is exercised: create, list, render, validate, openapi and authorization")]
    private async Task WhenAsync()
    {
        var client = _development.CreateClient();
        var created = await client.PostAsJsonAsync("/api/templates", new
        {
            Name = "EntityClass",
            Versions = new[] { new { Name = "v1", Content = "Hello ${Name}!", Json = """{"Name":"Ada"}""", Text = "Hello Ada!" } },
        });
        var version = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("versions")[0].GetProperty("id").GetGuid();
        var listed = await client.GetAsync("/api/templates");
        useCase.Cases["CreateStatus"] = created.StatusCode;
        useCase.Cases["ListStatus"] = listed.StatusCode;
        useCase.Cases["ListedCount"] = (await listed.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength();
        var rendered = await client.PostAsJsonAsync("/api/templates/render", new { Template = "Hello ${Name}!", Json = """{"Name":"Ada"}""" });
        useCase.Cases["RenderOutput"] = (await rendered.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("output").GetString();
        var validated = await client.PostAsync($"/api/templates/versions/{version}/validate", null);
        useCase.Cases["Validated"] = (await validated.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("valid").GetBoolean();
        var openApi = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        useCase.Cases["OpenApiPathCount"] = openApi.GetProperty("paths").EnumerateObject().Count();
        useCase.Cases["AnonymousStatus"] = (await _production.CreateClient().GetAsync("/api/templates")).StatusCode;
        var keyed = _production.CreateClient();
        keyed.DefaultRequestHeaders.Add("X-Api-Key", "secret-key");
        useCase.Cases["KeyedStatus"] = (await keyed.GetAsync("/api/templates")).StatusCode;
        using var database = _development.CreateDbContext();
        useCase.Cases["ProviderName"] = database.Database.ProviderName;
    }

    public ValueTask InitializeAsync()
    {
        useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult());
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _development?.Dispose();
        _production?.Dispose();
        return ValueTask.CompletedTask;
    }
    [Then("the list status should be OK")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ListReturnsOk() => ((HttpStatusCode)useCase.Cases["ListStatus"]!).Should().Be(HttpStatusCode.OK);
    [Then("the create status should be Created")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CreateReturnsCreated() => ((HttpStatusCode)useCase.Cases["CreateStatus"]!).Should().Be(HttpStatusCode.Created);
    [Then("the listed templates should have count 1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ListContainsTheCreatedTemplate() => ((int)useCase.Cases["ListedCount"]!).Should().Be(1);
    [Then("the render output should contain Hello Ada")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersThroughThisRepositorysEngine() => ((string?)useCase.Cases["RenderOutput"]!).Should().Contain("Hello Ada");
    [Then("the validation should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ValidatesTheVersion() => ((bool)useCase.Cases["Validated"]!).Should().Be(true);
    [Then("the open api paths should have count 4")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_PublishesTheDocumentedPaths() => ((int)useCase.Cases["OpenApiPathCount"]!).Should().Be(4);
    [Then("the anonymous production call should be unauthorized")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RefusesProductionWithoutAKey() => ((HttpStatusCode)useCase.Cases["AnonymousStatus"]!).Should().Be(HttpStatusCode.Unauthorized);
    [Then("the keyed production call should be OK")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_AcceptsProductionWithTheKey() => ((HttpStatusCode)useCase.Cases["KeyedStatus"]!).Should().Be(HttpStatusCode.OK);
    [Then("the provider should contain Sqlite")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_SelectsTheScenarioProvider() => ((string?)useCase.Cases["ProviderName"]!).Should().Contain("Sqlite");
}
#endif
