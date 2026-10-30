#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "05")]
[UserStory("01", "Template API CRUD operations")]
public sealed class SC05_ValidateVersion : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;
    private bool _valid;
    private HttpStatusCode _unknownStatus;
    private HttpStatusCode _missingDataStatus;

    [Given("a template library seeded with two templates")]
    private Task GivenAsync() => SeedLibraryAsync();

    [When("POST /api/templates/versions/{id}/validate is called with EntityClass v1")]
    private async Task WhenAsync()
    {
        _response = await Client.PostAsync($"/api/templates/versions/{EntityClassV1Id}/validate", null);
        _valid = (await _response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("valid").GetBoolean();

        _unknownStatus = (await Client.PostAsync($"/api/templates/versions/{Guid.NewGuid()}/validate", null)).StatusCode;
        _missingDataStatus = (await Client.PostAsync($"/api/templates/versions/{DbContextClassV1Id}/validate", null)).StatusCode;
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await GivenAsync();
        await WhenAsync();
    }

    [Then("the version should be valid")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ResponseIsValid() => _valid.Should().BeTrue();

    [Then("the unknown version should be NotFound")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_UnknownVersionReturns404() => _unknownStatus.Should().Be(HttpStatusCode.NotFound);

    [Then("the version without test data should be rejected")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_MissingTestDataReturns400() => _missingDataStatus.Should().Be(HttpStatusCode.BadRequest);
}
#endif
