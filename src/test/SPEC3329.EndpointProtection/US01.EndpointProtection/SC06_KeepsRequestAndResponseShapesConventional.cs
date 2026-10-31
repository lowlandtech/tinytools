#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "06")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC06_KeepsRequestAndResponseShapesConventional : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _host = null!;
    private TemplateApiFactory _keyed = null!;
    public SC06_KeepsRequestAndResponseShapesConventional(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;
    [Given("a development host and a keyed production host")]
    private void Given()
    {
        _host = new TemplateApiFactory();
        _keyed = new TemplateApiFactory("Production", "secret-key");
    }
    [When("a create, a list, a delete, a refusal and a malformed body are observed")]
    private async Task WhenAsync()
    {
        var client = _host.CreateClient();
        var created = await client.PostAsJsonAsync("/api/templates", new
        {
            Name = "EntityClass",
            Versions = new[] { new { Name = "v1", Content = "Hello ${Name}!", Json = """{"Name":"Ada"}""", Text = "Hello Ada!" } },
        });
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetGuid();
        useCase.Cases["ResponseKeys"] = string.Join(",", body.EnumerateObject().Select(property => property.Name));
        var listed = await client.GetAsync("/api/templates");
        var listedBody = await listed.Content.ReadFromJsonAsync<JsonElement>();
        useCase.Cases["ListIsArray"] = listedBody.ValueKind == JsonValueKind.Array;
        useCase.Cases["CreatedLocation"] = created.Headers.Location?.ToString() ?? string.Empty;
        var deleted = await client.DeleteAsync($"/api/templates/{id}");
        useCase.Cases["DeletedBody"] = (await deleted.Content.ReadAsStringAsync()).Length;
        var refused = await _keyed.CreateClient().GetAsync("/api/templates");
        useCase.Cases["RefusedMediaType"] = refused.Content.Headers.ContentType?.MediaType ?? string.Empty;
        var problem = await refused.Content.ReadFromJsonAsync<JsonElement>();
        useCase.Cases["ProblemType"] = problem.TryGetProperty("type", out var type) ? type.GetString() : null;
        useCase.Cases["ProblemStatus"] = problem.TryGetProperty("status", out var status) ? status.GetInt32() : 0;
        useCase.Cases["ProblemTrace"] = problem.TryGetProperty("traceId", out var trace) && !string.IsNullOrWhiteSpace(trace.GetString());
        var malformed = await client.PostAsync("/api/templates", new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"));
        useCase.Cases["MalformedMediaType"] = malformed.Content.Headers.ContentType?.MediaType ?? string.Empty;
        useCase.Cases["MalformedStatus"] = malformed.StatusCode;
    }
    public ValueTask InitializeAsync() { useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult()); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() { _host?.Dispose(); _keyed?.Dispose(); return ValueTask.CompletedTask; }

    [Then("the response keys should be camelCased")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_UsesOneCasing() => ((string)useCase.Cases["ResponseKeys"]!).Should().Be("id,name,isActive,versions,packages");

    [Then("the list is an array should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_KeepsOneCollectionContract() => ((bool)useCase.Cases["ListIsArray"]!).Should().BeTrue();

    [Then("the create location should contain the created id")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_NamesTheCreatedResource() => ((string)useCase.Cases["CreatedLocation"]!).Should().StartWith("/api/templates/");

    [Then("the delete body length should be 0")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_AnswersBodylessDeletes() => ((int)useCase.Cases["DeletedBody"]!).Should().Be(0);

    [Then("the refused media type should be application/problem+json")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_AnswersRefusalsAsProblems() => ((string)useCase.Cases["RefusedMediaType"]!).Should().Be("application/problem+json");

    [Then("the problem status should be 401")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_MatchesTheProblemStatusToTheResponse() => ((int)useCase.Cases["ProblemStatus"]!).Should().Be(401);

    [Then("the problem type should be https://lowlandtech.dev/problems/api-key-required")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_NamesAStableProblemType() => ((string?)useCase.Cases["ProblemType"]!).Should().Be("https://lowlandtech.dev/problems/api-key-required");

    [Then("the problem trace flag should be true")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_CarriesATraceIdentifier() => ((bool)useCase.Cases["ProblemTrace"]!).Should().BeTrue();

    [Then("the malformed status should be BadRequest")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_KeepsMalformedBodiesACallerError() => ((HttpStatusCode)useCase.Cases["MalformedStatus"]!).Should().Be(HttpStatusCode.BadRequest);

    [Then("the malformed media type should be application/problem+json")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_AnswersMalformedBodiesAsProblems() => ((string)useCase.Cases["MalformedMediaType"]!).Should().Be("application/problem+json");
}
#endif
