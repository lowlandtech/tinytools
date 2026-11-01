#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LowlandTech.TinyTools.Api.Endpoints;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
using Microsoft.Extensions.DependencyInjection;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "04")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC04_ProtectsTheEndpoints : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _host = null!;
    public SC04_ProtectsTheEndpoints(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;
    [Given("a host whose breaker opens after one second")]
    private void Given() => _host = new TemplateApiFactory("Development", null,
        new Dictionary<string, string?> { ["Tinytools:BreakerOpenSeconds"] = "1" });
    [When("sixty-one calls arrive, the store breaker trips, and an oversized render is sent")]
    private async Task WhenAsync()
    {
        var client = _host.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "quota-key");
        var statuses = new List<HttpStatusCode>();
        for (var call = 0; call < RateLimitMiddleware.Limit + 1; call++)
            statuses.Add((await client.GetAsync("/api/templates")).StatusCode);
        useCase.Cases["Admitted"] = statuses.Count(status => status == HttpStatusCode.OK);
        useCase.Cases["Refused"] = statuses.Count(status => status == HttpStatusCode.TooManyRequests);
        var refused = await client.GetAsync("/api/templates");
        useCase.Cases["RetryHint"] = (refused.Headers.RetryAfter?.Delta?.TotalSeconds ?? 0) > 0;
        var problem = await refused.Content.ReadFromJsonAsync<JsonElement>();
        useCase.Cases["RateType"] = problem.GetProperty("type").GetString();
        // Another credential is unaffected by the first credential's quota.
        var other = _host.CreateClient();
        other.DefaultRequestHeaders.Add("X-Api-Key", "other-key");
        useCase.Cases["OtherCredential"] = (await other.GetAsync("/api/templates")).StatusCode;
        var breaker = _host.Services.GetRequiredService<StoreBreaker>();
        breaker.RecordFailure();
        breaker.RecordFailure();
        useCase.Cases["StateAfterTwo"] = breaker.State;
        breaker.RecordFailure();
        useCase.Cases["StateAfterThree"] = breaker.State;
        var fresh = _host.CreateClient();
        fresh.DefaultRequestHeaders.Add("X-Api-Key", "breaker-key");
        var open = await fresh.GetAsync("/api/templates");
        useCase.Cases["OpenStatus"] = open.StatusCode;
        useCase.Cases["OpenType"] = (await open.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("type").GetString();
        await Task.Delay(1200);
        useCase.Cases["HalfOpen"] = breaker.State;
        useCase.Cases["ProbeStatus"] = (await fresh.GetAsync("/api/templates")).StatusCode;
        breaker.RecordSuccess();
        useCase.Cases["Closed"] = breaker.State;
    }
    public ValueTask InitializeAsync() { useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult()); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() { _host?.Dispose(); return ValueTask.CompletedTask; }
    [Then("the admitted count should be 60")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_AdmitsTheBudget() => ((int)useCase.Cases["Admitted"]!).Should().Be(RateLimitMiddleware.Limit);
    [Then("the refused count should be 1")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RefusesTheCallOverTheBudget() => ((int)useCase.Cases["Refused"]!).Should().Be(1);
    [Then("the retry hint flag should be true")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_NamesTheRetryHint() => ((bool)useCase.Cases["RetryHint"]!).Should().BeTrue();
    [Then("the rate type should be named")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_NamesTheRateRefusal() => ((string?)useCase.Cases["RateType"]!).Should().Be(RateLimitMiddleware.TooMany);
    [Then("the other credential status should be OK")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_PartitionsTheBudgetPerCredential() => ((HttpStatusCode)useCase.Cases["OtherCredential"]!).Should().Be(HttpStatusCode.OK);
    [Then("the state after two should be closed")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_KeepsTheBreakerClosedBelowTheThreshold() => ((string)useCase.Cases["StateAfterTwo"]!).Should().Be("closed");
    [Then("the state after three should be open")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_OpensOnTheThreshold() => ((string)useCase.Cases["StateAfterThree"]!).Should().Be("open");
    [Then("the open status should be ServiceUnavailable")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RefusesWhileTheStoreIsOpen() => ((HttpStatusCode)useCase.Cases["OpenStatus"]!).Should().Be(HttpStatusCode.ServiceUnavailable);
    [Then("the open type should be named")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_NamesTheBreakerRefusal() => ((string?)useCase.Cases["OpenType"]!).Should().Be(StoreBreaker.Unavailable);
    [Then("the half open state should be half-open")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_AdmitsAProbeAfterTheInterval() => ((string)useCase.Cases["HalfOpen"]!).Should().Be("half-open");
    [Then("the probe status should be OK")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_LetsTheProbeThrough() => ((HttpStatusCode)useCase.Cases["ProbeStatus"]!).Should().Be(HttpStatusCode.OK);
    [Then("the closed state should be closed")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_ClosesOnSuccess() => ((string)useCase.Cases["Closed"]!).Should().Be("closed");
}
#endif
