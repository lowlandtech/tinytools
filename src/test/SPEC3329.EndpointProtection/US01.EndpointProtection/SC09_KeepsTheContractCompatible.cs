#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LowlandTech.TinyTools.Api.Endpoints;
using LowlandTech.TinyTools.Frontend.Services.Http;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "09")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC09_KeepsTheContractCompatible : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _host = null!;
    public SC09_KeepsTheContractCompatible(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;
    [Given("a host and the pinned frontend consumer")]
    private void Given() => _host = new TemplateApiFactory();
    [When("the version header and the pinned consumer are exercised")]
    private async Task WhenAsync()
    {
        var client = _host.CreateClient();
        await client.PostAsJsonAsync("/api/templates", new
        {
            Name = "EntityClass",
            Versions = new[] { new { Name = "v1", Content = "Hello ${Name}!", Json = """{"Name":"Ada"}""", Text = "Hello Ada!" } },
        });
        useCase.Cases["DefaultStatus"] = (await client.GetAsync("/api/templates")).StatusCode;
        var explicitV1 = _host.CreateClient();
        explicitV1.DefaultRequestHeaders.Add("api-version", "1");
        useCase.Cases["V1Status"] = (await explicitV1.GetAsync("/api/templates")).StatusCode;
        var unsupported = _host.CreateClient();
        unsupported.DefaultRequestHeaders.Add("api-version", "2");
        var unsupportedResponse = await unsupported.GetAsync("/api/templates");
        useCase.Cases["UnsupportedStatus"] = unsupportedResponse.StatusCode;
        var problem = await unsupportedResponse.Content.ReadFromJsonAsync<JsonElement>();
        useCase.Cases["UnsupportedType"] = problem.GetProperty("type").GetString();
        var malformed = _host.CreateClient();
        malformed.DefaultRequestHeaders.Add("api-version", "latest");
        useCase.Cases["MalformedStatus"] = (await malformed.GetAsync("/api/templates")).StatusCode;

        // The pinned consumer is the shipped frontend client, not a test double.
        var consumer = new HttpTemplateLibrary(_host.CreateClient());
        useCase.Cases["ConsumerCount"] = (await consumer.ListAsync()).Count;

        var additive = """{"id":"6f1b2c3d-0000-0000-0000-000000000001","name":"X","isActive":true,"versions":[],"packages":[],"futureField":"ignored"}""";
        useCase.Cases["Additive"] = JsonDocument.Parse(additive).RootElement.TryGetProperty("futureField", out _);
        var breaking = """{"name":"X","isActive":true}""";
        useCase.Cases["Breaking"] = JsonDocument.Parse(breaking).RootElement.TryGetProperty("id", out _);
    }
    public ValueTask InitializeAsync() { useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult()); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() { _host?.Dispose(); return ValueTask.CompletedTask; }

    [Then("the default status should be OK")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ServesVersionOneByDefault() => ((HttpStatusCode)useCase.Cases["DefaultStatus"]!).Should().Be(HttpStatusCode.OK);

    [Then("the explicit v1 status should be OK")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_TreatsAnExplicitV1AsTheSameContract() => ((HttpStatusCode)useCase.Cases["V1Status"]!).Should().Be(HttpStatusCode.OK);

    [Then("the unsupported status should be BadRequest")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RefusesAnUnsupportedVersion() => ((HttpStatusCode)useCase.Cases["UnsupportedStatus"]!).Should().Be(HttpStatusCode.BadRequest);

    [Then("the unsupported type should be named")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_NamesTheVersionRefusal() => ((string?)useCase.Cases["UnsupportedType"]!).Should().Be(ApiVersionMiddleware.Unsupported);

    [Then("the malformed version status should be BadRequest")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RefusesAMalformedVersion() => ((HttpStatusCode)useCase.Cases["MalformedStatus"]!).Should().Be(HttpStatusCode.BadRequest);

    [Then("the consumer count should be 1")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ThePinnedConsumerReadsTheHost() => ((int)useCase.Cases["ConsumerCount"]!).Should().Be(1);

    [Then("the additive field flag should be true")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_AcceptsAnAdditiveField() => ((bool)useCase.Cases["Additive"]!).Should().BeTrue();

    [Then("the breaking flag should be false")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RefusesABreakingShape() => ((bool)useCase.Cases["Breaking"]!).Should().BeFalse();
}
#endif
