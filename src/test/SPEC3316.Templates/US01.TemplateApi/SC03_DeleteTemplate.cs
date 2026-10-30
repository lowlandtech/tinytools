#if NET10_0_OR_GREATER
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template API CRUD operations")]
public class SC03_DeleteTemplate : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;

    [When("DELETE /api/templates/{id} is called with the EntityClass ID")]
    protected override async Task WhenAsync()
    {
        _response = await Client.DeleteAsync($"/api/templates/{EntityClassId}");
    }

    [Fact(DisplayName = "UAC01: Response is 204 NoContent")]
    [Trait(Spec.UAC, "01")]
    [Then("the response status code is 204")]
    public async Task UAC01_ResponseIs204()
    {
        await ArrangeAndActAsync();
        _response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "UAC02: Template no longer in database")]
    [Trait(Spec.UAC, "02")]
    [Then("the database contains 0 templates with EntityClass ID")]
    public async Task UAC02_TemplateRemoved()
    {
        await ArrangeAndActAsync();

        await using var database = CreateDbContext();
        var exists = await database.Templates.AnyAsync(row => row.Id == EntityClassId);
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "UAC03: DELETE unknown ID returns 404")]
    [Trait(Spec.UAC, "03")]
    [Then("the response status code is 404 for an unknown ID")]
    public async Task UAC03_UnknownIdReturns404()
    {
        await ArrangeAndActAsync();

        var notFound = await Client.DeleteAsync($"/api/templates/{Guid.NewGuid()}");
        notFound.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
#endif
