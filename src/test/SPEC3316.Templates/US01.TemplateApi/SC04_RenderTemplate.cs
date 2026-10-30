#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "04")]
[UserStory("01", "Template API CRUD operations")]
public class SC04_RenderTemplate : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;

    [When("POST /api/tinytools/templates/render is called with template and JSON")]
    protected override async Task WhenAsync()
    {
        _response = await Client.PostAsJsonAsync("/api/tinytools/templates/render", new
        {
            Template = "Hello ${Context.Name}",
            Json = """{"Name":"World"}""",
        });
    }

    [Fact(DisplayName = "UAC01: Response is 200 OK")]
    [Trait(Spec.UAC, "01")]
    [Then("the response status code is 200")]
    public async Task UAC01_ResponseIs200()
    {
        await ArrangeAndActAsync();
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "UAC02: Output contains rendered text")]
    [Trait(Spec.UAC, "02")]
    [Then("the response output is 'Hello World'")]
    public async Task UAC02_OutputContainsRenderedText()
    {
        await ArrangeAndActAsync();

        using var document = JsonDocument.Parse(await _response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("output").GetString().Should().Be("Hello World");
    }
}
#endif
