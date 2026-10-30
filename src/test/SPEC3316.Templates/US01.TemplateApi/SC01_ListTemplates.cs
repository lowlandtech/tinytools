#if NET10_0_OR_GREATER
using System.Net;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template API CRUD operations")]
public class SC01_ListTemplates : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;

    [When("GET /api/templates is called")]
    protected override async Task WhenAsync()
    {
        _response = await Client.GetAsync("/api/templates");
    }

    [Fact(DisplayName = "UAC01: Response is 200 OK")]
    [Trait(Spec.UAC, "01")]
    [Then("the response status code is 200")]
    public async Task UAC01_ResponseIs200()
    {
        await ArrangeAndActAsync();
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "UAC02: Returns 2 templates")]
    [Trait(Spec.UAC, "02")]
    [Then("the response contains 2 templates")]
    public async Task UAC02_Returns2Templates()
    {
        await ArrangeAndActAsync();

        using var document = JsonDocument.Parse(await _response.Content.ReadAsStringAsync());
        document.RootElement.GetArrayLength().Should().Be(2);
    }
}
#endif
