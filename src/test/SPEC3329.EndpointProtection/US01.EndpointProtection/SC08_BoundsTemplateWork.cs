#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LowlandTech.TinyTools.Api.Endpoints;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;
namespace LowlandTech.TinyTools.Tests.SPEC3329.EndpointProtection.US01.EndpointProtection;
[Trait(Spec.SPEC, "3329")]
[Trait(Spec.SC, "08")]
[UserStory("01", "Every template endpoint is proven with a real request")]
public sealed class SC08_BoundsTemplateWork : IAsyncLifetime, IClassFixture<UseCases.EndpointProtectionUseCase>
{
    private readonly UseCases.EndpointProtectionUseCase useCase;
    private TemplateApiFactory _host = null!;
    public SC08_BoundsTemplateWork(UseCases.EndpointProtectionUseCase useCase) => this.useCase = useCase;
    [Given("a host and a render operation with declared byte bounds")]
    private void Given() => _host = new TemplateApiFactory();

    [When("payloads at and beyond the bounds are rendered and the collection is read")]
    private async Task WhenAsync()
    {
        var client = _host.CreateClient();
        static string Filler(int bytes) => new string('a', bytes);
        var atTemplate = await client.PostAsJsonAsync("/api/templates/render",
            new { Template = Filler(TemplateBounds.TemplateBytes), Json = "{}" });
        var atModel = await client.PostAsJsonAsync("/api/templates/render",
            new { Template = "x", Json = "{\"a\":\"" + Filler(TemplateBounds.ModelBytes - 8) + "\"}" });
        var overTemplate = await client.PostAsJsonAsync("/api/templates/render",
            new { Template = Filler(TemplateBounds.TemplateBytes + 1), Json = "{}" });
        var overModel = await client.PostAsJsonAsync("/api/templates/render",
            new { Template = "x", Json = "{\"a\":\"" + Filler(TemplateBounds.ModelBytes) + "\"}" });
        useCase.Cases["AtTemplateStatus"] = atTemplate.StatusCode;
        useCase.Cases["AtModelStatus"] = atModel.StatusCode;
        useCase.Cases["OverTemplateStatus"] = overTemplate.StatusCode;
        useCase.Cases["OverModelStatus"] = overModel.StatusCode;
        var problem = await overTemplate.Content.ReadFromJsonAsync<JsonElement>();
        useCase.Cases["OverType"] = problem.GetProperty("type").GetString();

        foreach (var name in new[] { "Zeta", "alpha", "Beta" })
            await client.PostAsJsonAsync("/api/templates", new
            {
                Name = name,
                Versions = new[] { new { Name = "v1", Content = "x", Json = "{}", Text = "x" } },
            });
        var listed = await client.GetFromJsonAsync<JsonElement>("/api/templates?page=2&cursor=zzz&limit=1");
        useCase.Cases["ListCount"] = listed.GetArrayLength();
        useCase.Cases["ListOrder"] = string.Join(",", listed.EnumerateArray().Select(entry => entry.GetProperty("name").GetString()));
        useCase.Cases["StoredAfterRender"] = listed.GetArrayLength();
    }

    public ValueTask InitializeAsync() { useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult()); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() { _host?.Dispose(); return ValueTask.CompletedTask; }

    [Then("the at bound template status should be OK")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_AcceptsATemplateAtTheBound() => ((HttpStatusCode)useCase.Cases["AtTemplateStatus"]!).Should().Be(HttpStatusCode.OK);

    [Then("the at bound model status should be OK")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_AcceptsAModelAtTheBound() => ((HttpStatusCode)useCase.Cases["AtModelStatus"]!).Should().Be(HttpStatusCode.OK);

    [Then("the over bound template status should be RequestEntityTooLarge")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RefusesATemplateOneByteOver() => ((HttpStatusCode)useCase.Cases["OverTemplateStatus"]!).Should().Be(HttpStatusCode.RequestEntityTooLarge);

    [Then("the over bound model status should be RequestEntityTooLarge")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RefusesAModelOneByteOver() => ((HttpStatusCode)useCase.Cases["OverModelStatus"]!).Should().Be(HttpStatusCode.RequestEntityTooLarge);

    [Then("the over bound type should be named")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_NamesTheBoundRefusal() => ((string?)useCase.Cases["OverType"]!).Should().Be(TemplateBounds.TooLarge);

    [Then("the list order should be Beta,Zeta,alpha")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_KeepsTheCollectionOrderedAndUnpaged() => ((string)useCase.Cases["ListOrder"]!).Should().Be("Beta,Zeta,alpha");

    [Then("the stored after render count should be 3")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_LeavesRenderWithoutEffects() => ((int)useCase.Cases["StoredAfterRender"]!).Should().Be(3);
}
#endif
