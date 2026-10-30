#if NET10_0_OR_GREATER
using System.Net;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "05")]
[UserStory("01", "Template API CRUD operations")]
public class SC05_ValidateVersion : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;

    [When("POST /api/templates/versions/{id}/validate is called with EntityClass v1")]
    protected override async Task WhenAsync()
    {
        _response = await Client.PostAsync($"/api/templates/versions/{EntityClassV1Id}/validate", null);
    }

    [Fact(DisplayName = "UAC01: Response is 200 OK with valid=true")]
    [Trait(Spec.UAC, "01")]
    [Then("the response is 200 with valid=true")]
    public async Task UAC01_ResponseIsValid()
    {
        await ArrangeAndActAsync();
        _response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await _response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("valid").GetBoolean().Should().BeTrue();
    }

    [Fact(DisplayName = "UAC02: Unknown version ID returns 404")]
    [Trait(Spec.UAC, "02")]
    [Then("the response status code is 404 for an unknown version ID")]
    public async Task UAC02_UnknownVersionReturns404()
    {
        await ArrangeAndActAsync();

        var notFound = await Client.PostAsync($"/api/templates/versions/{Guid.NewGuid()}/validate", null);
        notFound.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "UAC03: Version without Json/Text returns 400")]
    [Trait(Spec.UAC, "03")]
    [Then("the response status code is 400 for a version without test data")]
    public async Task UAC03_MissingTestDataReturns400()
    {
        await ArrangeAndActAsync();

        var badRequest = await Client.PostAsync($"/api/templates/versions/{DbContextClassV1Id}/validate", null);
        badRequest.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
#endif
