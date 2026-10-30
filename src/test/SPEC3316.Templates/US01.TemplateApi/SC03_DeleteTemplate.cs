#if NET10_0_OR_GREATER
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template API CRUD operations")]
public sealed class SC03_DeleteTemplate : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;
    private bool _remaining;
    private HttpStatusCode _unknownStatus;

    [Given("a template library seeded with two templates")]
    private Task GivenAsync() => SeedLibraryAsync();

    [When("DELETE /api/templates/{id} is called with the EntityClass ID")]
    private async Task WhenAsync()
    {
        _response = await Client.DeleteAsync($"/api/templates/{EntityClassId}");

        await using var database = CreateDbContext();
        _remaining = await database.Templates.AnyAsync(row => row.Id == EntityClassId);

        _unknownStatus = (await Client.DeleteAsync($"/api/templates/{Guid.NewGuid()}")).StatusCode;
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await GivenAsync();
        await WhenAsync();
    }

    [Then("the response status should be NoContent")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ResponseIs204() => _response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    [Then("the deleted template should be gone")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_TemplateRemoved() => _remaining.Should().BeFalse();

    [Then("the unknown delete should be NotFound")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_UnknownIdReturns404() => _unknownStatus.Should().Be(HttpStatusCode.NotFound);
}
#endif
