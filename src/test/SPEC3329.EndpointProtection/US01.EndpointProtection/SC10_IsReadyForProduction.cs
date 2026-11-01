#if NET10_0_OR_GREATER
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using LowlandTech.TinyTools.Api.Authorization;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
using Microsoft.Extensions.DependencyInjection;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "10")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC10_IsReadyForProduction : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _host = null!;
    public SC10_IsReadyForProduction(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;
    [Given("a host started in production mode with a configured key")]
    private void Given() => _host = new TemplateApiFactory("Production", "prod-key");
    [When("every operation is smoked and its trace, headers and latency are recorded")]
    private async Task WhenAsync()
    {
        var client = _host.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "prod-key");
        var log = _host.Services.GetRequiredService<EnforcementObservationLog>();
        var created = await client.PostAsJsonAsync("/api/templates", new
        {
            Name = "Smoke",
            Versions = new[] { new { Name = "v1", Content = "Hello ${Name}!", Json = """{"Name":"Ada"}""", Text = "Hello Ada!" } },
        });
        var body = await created.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var id = body.GetProperty("id").GetGuid();
        var version = body.GetProperty("versions")[0].GetProperty("id").GetGuid();
        var traceHeader = created.Headers.TryGetValues("X-Trace-Id", out var traceValues) ? traceValues.First() : null;
        var recorded = log.Snapshot().LastOrDefault(entry => entry.TraceId == traceHeader);
        useCase.Cases["TraceHeader"] = traceHeader ?? string.Empty;
        useCase.Cases["TraceRecorded"] = recorded is not null && recorded.Decision == "accepted";
        var operations = new (string Name, Func<Task<HttpResponseMessage>> Call)[]
        {
            ("list", () => client.GetAsync("/api/templates")),
            ("get", () => client.GetAsync($"/api/templates/{id}")),
            ("create", () => client.PostAsJsonAsync("/api/templates", new { Name = "Second", Versions = Array.Empty<object>() })),
            ("update", () => client.PutAsJsonAsync($"/api/templates/{id}", new { Name = "Smoke2", Versions = Array.Empty<object>() })),
            ("render", () => client.PostAsJsonAsync("/api/templates/render", new { Template = "x", Json = "{}" })),
            ("validate", () => client.PostAsync($"/api/templates/versions/{version}/validate", null)),
            ("delete", () => client.DeleteAsync($"/api/templates/{id}")),
        };
        var statuses = new List<int>();
        var durations = new List<double>();
        foreach (var operation in operations)
        {
            var watch = Stopwatch.StartNew();
            var response = await operation.Call();
            watch.Stop();
            statuses.Add((int)response.StatusCode);
            durations.Add(watch.Elapsed.TotalMilliseconds);
        }
        useCase.Cases["Succeeded"] = statuses.Count(status => status is >= 200 and < 300);
        useCase.Cases["WithinObjective"] = durations.Max() < 2000;
        useCase.Cases["AllRecorded"] = log.Snapshot().Count(entry => entry.Decision == "accepted") >= 7;
        var headers = created.Headers;
        useCase.Cases["CacheControl"] = string.Join(",", headers.CacheControl?.ToString() ?? string.Empty);
        useCase.Cases["ServerHeader"] = string.Join(",", headers.TryGetValues("Server", out var server) ? server : []);
        useCase.Cases["CorsHeader"] = string.Join(",", headers.TryGetValues("Access-Control-Allow-Origin", out var cors) ? cors : []);
    }
    public ValueTask InitializeAsync() { useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult()); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() { _host?.Dispose(); return ValueTask.CompletedTask; }
    [Then("the trace header should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReturnsTheTraceIdentifier() => ((string)useCase.Cases["TraceHeader"]!).Should().NotBeNullOrWhiteSpace();
    [Then("the trace recorded flag should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CorrelatesTheRequestWithItsDecision() => ((bool)useCase.Cases["TraceRecorded"]!).Should().BeTrue();
    [Then("the succeeded operations should have count 7")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_SmokesEveryOperation() => ((int)useCase.Cases["Succeeded"]!).Should().Be(7);
    [Then("the objective flag should be true")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_StatesAnObjectiveAndMeetsIt() => ((bool)useCase.Cases["WithinObjective"]!).Should().BeTrue();
    [Then("the recorded flag should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RecordsEveryOperation() => ((bool)useCase.Cases["AllRecorded"]!).Should().BeTrue();
    [Then("the cache control should contain no-store")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_DeclaresTheResponseUncacheable() => ((string)useCase.Cases["CacheControl"]!).Should().Contain("no-store");
    [Then("the server header should be empty")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ExposesNoImplementationHeader() => ((string)useCase.Cases["ServerHeader"]!).Should().BeEmpty();
    [Then("the cors header should be empty")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_AnswersNoCorsHeaderWithoutAPolicy() => ((string)useCase.Cases["CorsHeader"]!).Should().BeEmpty();
}
#endif
