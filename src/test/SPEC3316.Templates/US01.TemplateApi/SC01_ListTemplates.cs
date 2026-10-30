#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template API CRUD operations")]
public sealed class SC01_ListTemplates : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;
    private int _listedCount;

    [Given("a template library seeded with two templates")]
    private Task GivenAsync() => SeedLibraryAsync();

    [When("GET /api/templates is called")]
    private async Task WhenAsync()
    {
        _response = await Client.GetAsync("/api/templates");
        _listedCount = (await _response.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength();
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await GivenAsync();
        await WhenAsync();
    }

    [Then("the response status should be OK")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ResponseIs200() => _response.StatusCode.Should().Be(HttpStatusCode.OK);

    [Then("the listed templates should have count 2")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_Returns2Templates() => _listedCount.Should().Be(2);
}
#endif
