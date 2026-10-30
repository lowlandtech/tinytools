#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "01")]
[UserStory("02", "The server serves the template contract over HTTP")]
public sealed class SC01_ServesTemplateContract : IAsyncLifetime
{
    private TemplateApiFactory _development = null!;
    private TemplateApiFactory _production = null!;
    private HttpStatusCode _listStatus, _createStatus, _anonymousStatus, _keyedStatus;
    private int _listedCount, _openApiPathCount;
    private string? _renderOutput, _providerName;
    private bool _validated;

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
        _createStatus = created.StatusCode;
        _listStatus = listed.StatusCode;
        _listedCount = (await listed.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength();
        var rendered = await client.PostAsJsonAsync("/api/templates/render", new { Template = "Hello ${Name}!", Json = """{"Name":"Ada"}""" });
        _renderOutput = (await rendered.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("output").GetString();
        var validated = await client.PostAsync($"/api/templates/versions/{version}/validate", null);
        _validated = (await validated.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("valid").GetBoolean();
        var openApi = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        _openApiPathCount = openApi.GetProperty("paths").EnumerateObject().Count();
        _anonymousStatus = (await _production.CreateClient().GetAsync("/api/templates")).StatusCode;
        var keyed = _production.CreateClient();
        keyed.DefaultRequestHeaders.Add("X-Api-Key", "secret-key");
        _keyedStatus = (await keyed.GetAsync("/api/templates")).StatusCode;
        using var database = _development.CreateDbContext();
        _providerName = database.Database.ProviderName;
    }

    public async ValueTask InitializeAsync()
    {
        Given();
        await WhenAsync();
    }

    public ValueTask DisposeAsync()
    {
        _development?.Dispose();
        _production?.Dispose();
        return ValueTask.CompletedTask;
    }
    [Then("the list status should be OK")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ListReturnsOk() => _listStatus.Should().Be(HttpStatusCode.OK);
    [Then("the create status should be Created")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CreateReturnsCreated() => _createStatus.Should().Be(HttpStatusCode.Created);
    [Then("the listed templates should have count 1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ListContainsTheCreatedTemplate() => _listedCount.Should().Be(1);
    [Then("the render output should contain Hello Ada")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersThroughThisRepositorysEngine() => _renderOutput.Should().Contain("Hello Ada");
    [Then("the validation should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ValidatesTheVersion() => _validated.Should().BeTrue();
    [Then("the open api paths should have count 4")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_PublishesTheDocumentedPaths() => _openApiPathCount.Should().Be(4);
    [Then("the anonymous production call should be unauthorized")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RefusesProductionWithoutAKey() => _anonymousStatus.Should().Be(HttpStatusCode.Unauthorized);
    [Then("the keyed production call should be OK")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_AcceptsProductionWithTheKey() => _keyedStatus.Should().Be(HttpStatusCode.OK);
    [Then("the provider should contain Sqlite")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_SelectsTheScenarioProvider() => _providerName.Should().Contain("Sqlite");
}
#endif
