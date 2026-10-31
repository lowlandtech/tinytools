#if NET10_0_OR_GREATER
using System.Net;
using LowlandTech.TinyTools.Api.Authorization;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
using Microsoft.Extensions.DependencyInjection;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC03_RecordsProvenanceWithoutTrustingTheCaller : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _host = null!;
    private CallerProvenanceLog _log = null!;
    private Dictionary<string, string?> _settings = null!;
    public SC03_RecordsProvenanceWithoutTrustingTheCaller(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;

    [Given("a production host whose configured key can change while it runs")]
    private void Given()
    {
        _settings = new Dictionary<string, string?> { ["Tinytools:ApiKey"] = "first-key" };
        _host = new TemplateApiFactory("Production", null, _settings);
    }

    [When("an accepted call, a spoofed anonymous call and a rotated key are observed")]
    private async Task WhenAsync()
    {
        var accepted = _host.CreateClient();
        _log = _host.Services.GetRequiredService<CallerProvenanceLog>();
        accepted.DefaultRequestHeaders.Add("X-Api-Key", "first-key");
        await accepted.GetAsync("/api/templates");
        var acceptedProvenance = _log.Snapshot().Last();
        useCase.Cases["Credential"] = acceptedProvenance.Credential;
        useCase.Cases["Peer"] = acceptedProvenance.Peer;

        var spoofed = _host.CreateClient();
        spoofed.DefaultRequestHeaders.Add("X-User", "admin");
        spoofed.DefaultRequestHeaders.Add("X-Forwarded-For", "203.0.113.9");
        useCase.Cases["SpoofStatus"] = (await spoofed.GetAsync("/api/templates")).StatusCode;

        _settings["Tinytools:ApiKey"] = "second-key";
        await using (var scope = _host.Services.CreateAsyncScope())
        {
            await Task.Yield();
        }
        var superseded = _host.CreateClient();
        superseded.DefaultRequestHeaders.Add("X-Api-Key", "first-key");
        useCase.Cases["SupersededStatus"] = (await superseded.GetAsync("/api/templates")).StatusCode;
        var rotated = _host.CreateClient();
        rotated.DefaultRequestHeaders.Add("X-Api-Key", "second-key");
        useCase.Cases["RotatedStatus"] = (await rotated.GetAsync("/api/templates")).StatusCode;
        useCase.Cases["Peers"] = string.Join("|", _log.Snapshot().Select(entry => entry.Peer).Distinct());
        useCase.Cases["Refusals"] = _log.Snapshot().Count(entry => entry.Credential == "none");
    }

    public ValueTask InitializeAsync()
    {
        useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult());
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() { _host?.Dispose(); return ValueTask.CompletedTask; }

    [Then("the recorded credential should be api-key")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RecordsTheCredentialKind() => ((string)useCase.Cases["Credential"]!).Should().Be("api-key");

    [Then("the recorded peer should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RecordsTheTransportPeer() => string.IsNullOrWhiteSpace((string)useCase.Cases["Peer"]!).Should().BeFalse();

    [Then("the spoofed anonymous status should be Unauthorized")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_IgnoresClientAssertedIdentity() => ((HttpStatusCode)useCase.Cases["SpoofStatus"]!).Should().Be(HttpStatusCode.Unauthorized);

    [Then("the recorded peers should not contain 203.0.113.9")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_IgnoresClientForwardingHeaders() => ((string)useCase.Cases["Peers"]!).Should().NotContain("203.0.113.9");

    [Then("the superseded key status should be Unauthorized")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RetiresTheSupersededKey() => ((HttpStatusCode)useCase.Cases["SupersededStatus"]!).Should().Be(HttpStatusCode.Unauthorized);

    [Then("the rotated key status should be OK")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_AcceptsTheRotatedKeyWithoutARestart() => ((HttpStatusCode)useCase.Cases["RotatedStatus"]!).Should().Be(HttpStatusCode.OK);

    [Then("the anonymous refusals should have count 1")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RecordsRefusalsToo() => ((int)useCase.Cases["Refusals"]!).Should().Be(1);
}
#endif
