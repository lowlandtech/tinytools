#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC01_ProvesEveryTemplateEndpoint : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _host = null!;
    public SC01_ProvesEveryTemplateEndpoint(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;
    [Given("a template host with an isolated database")]
    private void Given() => _host = new TemplateApiFactory();
    [When("each operation runs with a valid request and each boundary case runs against it")]
    private async Task WhenAsync()
    {
        var client = _host.CreateClient();
        var anonymous = await client.GetAsync("/api/templates");
        useCase.Cases["ListStatus"] = anonymous.StatusCode;
        var created = await client.PostAsJsonAsync("/api/templates", new
        {
            Name = "EntityClass",
            Versions = new[] { new { Name = "v1", Content = "Hello ${Name}!", Json = """{"Name":"Ada"}""", Text = "Hello Ada!" } },
        });
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetGuid();
        var version = body.GetProperty("versions")[0].GetProperty("id").GetGuid();
        useCase.Cases["CreateStatus"] = created.StatusCode;
        useCase.Cases["CreateShape"] = body.TryGetProperty("versions", out _) && body.TryGetProperty("name", out _);
        var fetched = await client.GetAsync($"/api/templates/{id}");
        useCase.Cases["FetchedName"] = (await fetched.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("name").GetString();
        var updated = await client.PutAsJsonAsync($"/api/templates/{id}", new
        {
            Name = "Renamed",
            Versions = new[] { new { Name = "v1", Content = "Hello ${Name}!", Json = """{"Name":"Ada"}""", Text = "Hello Ada!" } },
        });
        useCase.Cases["UpdatedName"] = (await updated.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("name").GetString();
        var rendered = await client.PostAsJsonAsync("/api/templates/render", new { Template = "Hello ${Name}!", Json = """{"Name":"Ada"}""" });
        useCase.Cases["RenderOutput"] = (await rendered.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("output").GetString();
        var validated = await client.PostAsync($"/api/templates/versions/{version}/validate", null);
        useCase.Cases["ValidateValue"] = (await validated.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("valid").GetBoolean();
        useCase.Cases["UnknownIds"] = string.Join(",",
            (await client.GetAsync($"/api/templates/{Guid.NewGuid()}")).StatusCode,
            (await client.PutAsJsonAsync($"/api/templates/{Guid.NewGuid()}", new { Name = "x" })).StatusCode,
            (await client.DeleteAsync($"/api/templates/{Guid.NewGuid()}")).StatusCode);
        useCase.Cases["MalformedBody"] = (await client.PostAsync("/api/templates",
            new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"))).StatusCode;
        var deleted = await client.DeleteAsync($"/api/templates/{id}");
        useCase.Cases["Removed"] = $"{deleted.StatusCode},{(await client.GetAsync($"/api/templates/{id}")).StatusCode}";
        useCase.Cases["Succeeded"] = new[]
        {
            anonymous, created, fetched, updated, rendered, validated, deleted,
        }.Count(response => (int)response.StatusCode is >= 200 and < 300);
    }
    public ValueTask InitializeAsync()
    {
        useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult());
        return ValueTask.CompletedTask;
    }
    public ValueTask DisposeAsync() { _host?.Dispose(); return ValueTask.CompletedTask; }
    [Then("the list status should be OK")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ListsTemplates() => ((HttpStatusCode)useCase.Cases["ListStatus"]!).Should().Be(HttpStatusCode.OK);
    [Then("the create status should be Created")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CreatesATemplate() => ((HttpStatusCode)useCase.Cases["CreateStatus"]!).Should().Be(HttpStatusCode.Created);
    [Then("the create shape should be true")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReturnsALocatableResource() => ((bool)useCase.Cases["CreateShape"]!).Should().BeTrue();
    [Then("the fetched name should be EntityClass")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ReadsBackWhatItStored() => ((string?)useCase.Cases["FetchedName"]!).Should().Be("EntityClass");
    [Then("the updated name should be Renamed")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_UpdatesInPlace() => ((string?)useCase.Cases["UpdatedName"]!).Should().Be("Renamed");
    [Then("the render output should contain Hello Ada")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RendersThroughTheEngine() => ((string?)useCase.Cases["RenderOutput"]!).Should().Contain("Hello Ada");
    [Then("the validation result should be true")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ValidatesAVersion() => ((bool)useCase.Cases["ValidateValue"]!).Should().BeTrue();
    [Then("the unknown identifier statuses should be NotFound,NotFound,NotFound")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RefusesUnknownIdentifiers() => ((string?)useCase.Cases["UnknownIds"]!).Should().Be("NotFound,NotFound,NotFound");
    [Then("the malformed body status should be BadRequest")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_RefusesAMalformedBody() => ((HttpStatusCode)useCase.Cases["MalformedBody"]!).Should().Be(HttpStatusCode.BadRequest);
    [Then("the removed statuses should be NoContent,NotFound")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_DeletesTheStoredTemplate() => ((string?)useCase.Cases["Removed"]!).Should().Be("NoContent,NotFound");
    [Then("the succeeded operations should have count 7")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_CoversEveryOperation() => ((int)useCase.Cases["Succeeded"]!).Should().Be(7);
}
#endif
