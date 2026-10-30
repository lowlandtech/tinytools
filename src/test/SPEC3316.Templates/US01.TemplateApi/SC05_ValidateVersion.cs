#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "05")]
[UserStory("01", "Template API CRUD operations")]
public sealed class SC05_ValidateVersion : TemplateApiScenario, IClassFixture<UseCases.TemplateApiUseCase>
{
    private readonly UseCases.TemplateApiUseCase useCase;

    public SC05_ValidateVersion(UseCases.TemplateApiUseCase useCase) => this.useCase = useCase;

    [Given("a template library seeded with two templates")]
    private Task GivenAsync() => SeedLibraryAsync();

    [When("POST /api/templates/versions/{id}/validate is called with EntityClass v1")]
    private async Task WhenAsync()
    {
        var response = await Client.PostAsync($"/api/templates/versions/{EntityClassV1Id}/validate", null);
        useCase.Cases["Valid"] = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("valid").GetBoolean();

        useCase.Cases["UnknownStatus"] = (await Client.PostAsync($"/api/templates/versions/{Guid.NewGuid()}/validate", null)).StatusCode;
        useCase.Cases["MissingDataStatus"] = (await Client.PostAsync($"/api/templates/versions/{DbContextClassV1Id}/validate", null)).StatusCode;
    }

    public override ValueTask InitializeAsync()
    {
        useCase.RunOnce(
            () => { base.InitializeAsync(); GivenAsync().GetAwaiter().GetResult(); },
            () => WhenAsync().GetAwaiter().GetResult());
        return ValueTask.CompletedTask;
    }

    [Then("the version should be valid")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ResponseIsValid() => ((bool)useCase.Cases["Valid"]!).Should().Be(true);

    [Then("the unknown version should be NotFound")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_UnknownVersionReturns404() => ((HttpStatusCode)useCase.Cases["UnknownStatus"]!).Should().Be(HttpStatusCode.NotFound);

    [Then("the version without test data should be rejected")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_MissingTestDataReturns400() => ((HttpStatusCode)useCase.Cases["MissingDataStatus"]!).Should().Be(HttpStatusCode.BadRequest);
}
#endif
