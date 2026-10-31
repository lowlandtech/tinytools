#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC02_RefusesCallersWithoutTheKey : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _keyed = null!;
    private TemplateApiFactory _unkeyed = null!;
    private TemplateApiFactory _development = null!;
    public SC02_RefusesCallersWithoutTheKey(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;
    [Given("three hosts: production with a key, production without one, and development")]
    private void Given()
    {
        _keyed = new TemplateApiFactory("Production", "secret-key");
        _unkeyed = new TemplateApiFactory("Production");
        _development = new TemplateApiFactory();
    }
    [When("the template surface is called with a valid key, no key, a wrong key and in development")]
    private async Task WhenAsync()
    {
        var valid = _keyed.CreateClient();
        valid.DefaultRequestHeaders.Add("X-Api-Key", "secret-key");
        useCase.Cases["ValidStatus"] = (await valid.GetAsync("/api/templates")).StatusCode;
        var absent = await _keyed.CreateClient().GetAsync("/api/templates");
        useCase.Cases["AbsentStatus"] = absent.StatusCode;
        useCase.Cases["AbsentBody"] = await absent.Content.ReadAsStringAsync();
        var wrong = _keyed.CreateClient();
        wrong.DefaultRequestHeaders.Add("X-Api-Key", "not-the-key");
        var wrongResponse = await wrong.GetAsync("/api/templates");
        useCase.Cases["WrongStatus"] = wrongResponse.StatusCode;
        useCase.Cases["WrongBody"] = await wrongResponse.Content.ReadAsStringAsync();
        var cased = _keyed.CreateClient();
        cased.DefaultRequestHeaders.Add("X-Api-Key", "SECRET-KEY");
        useCase.Cases["CasedStatus"] = (await cased.GetAsync("/api/templates")).StatusCode;
        static HttpRequestMessage[] Requests() =>
        [
            new(HttpMethod.Get, "/api/templates"),
            new(HttpMethod.Get, $"/api/templates/{Guid.NewGuid()}"),
            new(HttpMethod.Post, "/api/templates") { Content = JsonContent.Create(new { Name = "x" }) },
            new(HttpMethod.Put, $"/api/templates/{Guid.NewGuid()}") { Content = JsonContent.Create(new { Name = "x" }) },
            new(HttpMethod.Delete, $"/api/templates/{Guid.NewGuid()}"),
            new(HttpMethod.Post, "/api/templates/render") { Content = JsonContent.Create(new { Template = "x", Json = "{}" }) },
            new(HttpMethod.Post, $"/api/templates/versions/{Guid.NewGuid()}/validate"),
        ];
        var anonymous = _keyed.CreateClient();
        var codes = new List<HttpStatusCode>();
        foreach (var call in Requests()) codes.Add((await anonymous.SendAsync(call)).StatusCode);
        useCase.Cases["Guarded"] = codes.Distinct().Count();
        useCase.Cases["Development"] = (await _development.CreateClient().GetAsync("/api/templates")).StatusCode;
        var unconfigured = _unkeyed.CreateClient();
        var unconfiguredCodes = new List<HttpStatusCode>();
        foreach (var call in Requests()) unconfiguredCodes.Add((await unconfigured.SendAsync(call)).StatusCode);
        useCase.Cases["UnconfiguredStatuses"] = string.Join(",", unconfiguredCodes);
    }
    public ValueTask InitializeAsync() { useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult()); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() { _keyed?.Dispose(); _unkeyed?.Dispose(); _development?.Dispose(); return ValueTask.CompletedTask; }
    [Then("the valid key status should be OK")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_AcceptsTheConfiguredKey() => ((HttpStatusCode)useCase.Cases["ValidStatus"]!).Should().Be(HttpStatusCode.OK);
    [Then("the absent key status should be Unauthorized")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RefusesAnAbsentKey() => ((HttpStatusCode)useCase.Cases["AbsentStatus"]!).Should().Be(HttpStatusCode.Unauthorized);
    [Then("the wrong key status should be Unauthorized")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RefusesAWrongKey() => ((HttpStatusCode)useCase.Cases["WrongStatus"]!).Should().Be(HttpStatusCode.Unauthorized);
    [Then("the wrong cased key status should be Unauthorized")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ComparesTheKeyExactly() => ((HttpStatusCode)useCase.Cases["CasedStatus"]!).Should().Be(HttpStatusCode.Unauthorized);
    [Then("the two refusal bodies should be identical")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_AnswersRefusalsTheSameWay() => ((string?)useCase.Cases["AbsentBody"]!).Should().Be((string?)useCase.Cases["WrongBody"]);
    [Then("the absent body should not contain secret-key")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_LeaksNothingAboutTheKey() => ((string?)useCase.Cases["AbsentBody"]!).Should().NotContain("secret-key");
    [Then("the unconfigured host statuses should be Unauthorized,Unauthorized,Unauthorized,Unauthorized,Unauthorized,Unauthorized,Unauthorized")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_FailsClosedWithoutAConfiguredKey() => ((string)useCase.Cases["UnconfiguredStatuses"]!).Should().Be("Unauthorized,Unauthorized,Unauthorized,Unauthorized,Unauthorized,Unauthorized,Unauthorized");
    [Then("the development anonymous status should be OK")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_DocumentsTheDevelopmentBypass() => ((HttpStatusCode)useCase.Cases["Development"]!).Should().Be(HttpStatusCode.OK);
    [Then("the guarded refusal kinds should have count 1")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_GuardsEveryOperation() => ((int)useCase.Cases["Guarded"]!).Should().Be(1);
}
#endif
