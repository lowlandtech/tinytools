#if NET10_0_OR_GREATER
using System.Net;
using LowlandTech.TinyTools.Api.Authorization;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
using Microsoft.Extensions.DependencyInjection;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "11")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC11_ObservesProtectionWithoutEnforcing : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _development = null!;
    private TemplateApiFactory _production = null!;
    private EnforcementObservationLog _log = null!;
    public SC11_ObservesProtectionWithoutEnforcing(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;
    [Given("a development host and a keyed production host")]
    private void Given()
    {
        _development = new TemplateApiFactory();
        _production = new TemplateApiFactory("Production", "secret-key");
    }
    [When("accepted, anonymous and concurrent calls are observed")]
    private async Task WhenAsync()
    {
        _log = _development.Services.GetRequiredService<EnforcementObservationLog>();
        var client = _development.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "secret-key");
        await client.GetAsync("/api/templates");
        var accepted = _log.Snapshot().Last();
        var spoofed = _development.CreateClient();
        spoofed.DefaultRequestHeaders.Add("X-User", "admin");
        spoofed.DefaultRequestHeaders.Add("X-Forwarded-For", "203.0.113.9");
        await spoofed.GetAsync("/api/templates");
        var anonymous = _log.Snapshot().Last();
        useCase.Cases["TraceId"] = accepted.TraceId;
        useCase.Cases["Credential"] = accepted.Credential;
        useCase.Cases["Decision"] = accepted.Decision;
        useCase.Cases["AnonymousDecision"] = anonymous.Decision;
        useCase.Cases["AnonymousWouldRefuse"] = anonymous.WouldRefuse;
        useCase.Cases["SpoofedCredential"] = anonymous.Credential;
        useCase.Cases["SpoofedPeer"] = anonymous.Peer;
        var produced = _production.CreateClient();
        var productionLog = _production.Services.GetRequiredService<EnforcementObservationLog>();
        useCase.Cases["EnforcedStatus"] = (await produced.GetAsync("/api/templates")).StatusCode;
        useCase.Cases["EnforcedRecorded"] = productionLog.Snapshot().Any(entry => entry.Decision == "refused");
        var concurrent = _development.CreateClient();
        concurrent.DefaultRequestHeaders.Add("X-Api-Key", "secret-key");
        await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => concurrent.GetAsync("/api/templates")));
        var traces = _log.Snapshot().Select(entry => entry.TraceId).Distinct().Count();
        useCase.Cases["DistinctTraces"] = traces;
        useCase.Cases["RecordedFields"] = string.Join("|", _log.Snapshot().Select(entry => $"{entry.Credential}:{entry.Peer}:{entry.Decision}"));
    }
    public ValueTask InitializeAsync() { useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult()); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() { _development?.Dispose(); _production?.Dispose(); return ValueTask.CompletedTask; }
    [Then("the trace id should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RecordsAnImmutableObservation() => ((string)useCase.Cases["TraceId"]!).Should().NotBeNullOrWhiteSpace();
    [Then("the credential should be api-key")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RecordsTheCredentialSource() => ((string)useCase.Cases["Credential"]!).Should().Be("api-key");
    [Then("the decision should be accepted")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RecordsTheDecision() => ((string)useCase.Cases["Decision"]!).Should().Be("accepted");
    [Then("the anonymous decision should be accepted")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ObservingChangesNothing() => ((string)useCase.Cases["AnonymousDecision"]!).Should().Be("accepted");
    [Then("the anonymous would refuse flag should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RecordsWhatWouldBeRefused() => ((bool)useCase.Cases["AnonymousWouldRefuse"]!).Should().BeTrue();
    [Then("the spoofed credential should be none")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_IgnoresClientAssertedIdentity() => ((string)useCase.Cases["SpoofedCredential"]!).Should().Be("none");
    [Then("the spoofed peer should not contain 203.0.113.9")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_IgnoresClientForwardingHeaders() => ((string)useCase.Cases["SpoofedPeer"]!).Should().NotContain("203.0.113.9");
    [Then("the enforced status should be Unauthorized")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_KeepsEnforcingWhereItAlreadyDid() => ((HttpStatusCode)useCase.Cases["EnforcedStatus"]!).Should().Be(HttpStatusCode.Unauthorized);
    [Then("the enforced record flag should be true")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_RecordsARefusal() => ((bool)useCase.Cases["EnforcedRecorded"]!).Should().BeTrue();
    [Then("the concurrent traces should have count 5")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_KeepsConcurrentObservationsDistinct() => ((int)useCase.Cases["DistinctTraces"]!).Should().Be(5);
    [Then("the recorded fields should not contain secret-key")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_RedactsWhatItRecords() => ((string)useCase.Cases["RecordedFields"]!).Should().NotContain("secret-key");
}
#endif
