#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "07")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC07_MakesWritesRetrySafe : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _host = null!;
    public SC07_MakesWritesRetrySafe(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;
    [Given("a host and a client that can send an idempotency key")]
    private void Given() => _host = new TemplateApiFactory();
    [When("a create is retried, a key is reused with another payload, and a stale update is attempted")]
    private async Task WhenAsync()
    {
        var client = _host.CreateClient();
        object Payload(string name) => new
        {
            Name = name,
            Versions = new[] { new { Name = "v1", Content = "Hello ${Name}!", Json = """{"Name":"Ada"}""", Text = "Hello Ada!" } },
        };
        client.DefaultRequestHeaders.Add("Idempotency-Key", "retry-1");
        var first = await client.PostAsJsonAsync("/api/templates", Payload("EntityClass"));
        var firstId = (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var retried = await client.PostAsJsonAsync("/api/templates", Payload("EntityClass"));
        var retriedId = (await retried.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        useCase.Cases["FirstStatus"] = first.StatusCode;
        useCase.Cases["RetriedSameId"] = retriedId == firstId;
        useCase.Cases["RetriedStatus"] = retried.StatusCode;
        var conflicted = await client.PostAsJsonAsync("/api/templates", Payload("OtherName"));
        useCase.Cases["ConflictStatus"] = conflicted.StatusCode;
        useCase.Cases["ConflictType"] = (await conflicted.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("type").GetString();

        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        var fetched = await client.GetAsync($"/api/templates/{firstId}");
        useCase.Cases["ETag"] = fetched.Headers.ETag?.Tag;
        useCase.Cases["CacheControl"] = fetched.Headers.CacheControl?.ToString() ?? string.Empty;

        var stale = _host.CreateClient();
        stale.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "\"stale-value\"");
        useCase.Cases["StaleStatus"] = (await stale.PutAsJsonAsync($"/api/templates/{firstId}", Payload("Renamed"))).StatusCode;

        var fresh = _host.CreateClient();
        fresh.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", useCase.Cases["ETag"]!.ToString());
        useCase.Cases["FreshStatus"] = (await fresh.PutAsJsonAsync($"/api/templates/{firstId}", Payload("Renamed"))).StatusCode;
    }
    public ValueTask InitializeAsync() { useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult()); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() { _host?.Dispose(); return ValueTask.CompletedTask; }

    [Then("the first status should be Created")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_CreatesOnce() => ((HttpStatusCode)useCase.Cases["FirstStatus"]!).Should().Be(HttpStatusCode.Created);

    [Then("the retried same id flag should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ReplaysARetry() => ((bool)useCase.Cases["RetriedSameId"]!).Should().BeTrue();

    [Then("the retried status should be OK")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_AnswersAReplayWithoutCreating() => ((HttpStatusCode)useCase.Cases["RetriedStatus"]!).Should().Be(HttpStatusCode.OK);

    [Then("the conflict status should be Conflict")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RefusesAReusedKey() => ((HttpStatusCode)useCase.Cases["ConflictStatus"]!).Should().Be(HttpStatusCode.Conflict);

    [Then("the conflict type should be named")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_NamesTheKeyRefusal() => ((string?)useCase.Cases["ConflictType"]!).Should().Be("https://lowlandtech.dev/problems/idempotency-key-reused");

    [Then("the etag should not be null")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_NamesTheCurrentState() => ((string?)useCase.Cases["ETag"]!).Should().NotBeNullOrWhiteSpace();

    [Then("the cache control should contain no-store")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_DeclaresTheResponseUncacheable() => ((string)useCase.Cases["CacheControl"]!).Should().Contain("no-store");

    [Then("the stale status should be PreconditionFailed")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RefusesAStaleWrite() => ((HttpStatusCode)useCase.Cases["StaleStatus"]!).Should().Be(HttpStatusCode.PreconditionFailed);

    [Then("the fresh status should be OK")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_AcceptsACurrentWrite() => ((HttpStatusCode)useCase.Cases["FreshStatus"]!).Should().Be(HttpStatusCode.OK);
}
#endif
