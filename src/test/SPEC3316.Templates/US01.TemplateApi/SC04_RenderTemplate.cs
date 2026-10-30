#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "04")]
[UserStory("01", "Template API CRUD operations")]
public sealed class SC04_RenderTemplate : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;
    private string? _output;

    [Given("a template library seeded with two templates")]
    private Task GivenAsync() => SeedLibraryAsync();

    [When("POST /api/templates/render is called with template and JSON")]
    private async Task WhenAsync()
    {
        _response = await Client.PostAsJsonAsync("/api/templates/render", new
        {
            Template = "Hello ${Context.Name}",
            Json = """{"Name":"World"}""",
        });
        _output = (await _response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("output").GetString();
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

    [Then("the rendered output should contain Hello World")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_OutputContainsRenderedText() => _output.Should().Contain("Hello World");
}
#endif
