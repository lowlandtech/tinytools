#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template API CRUD operations")]
public class SC02_CreateTemplate : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;

    [When("POST /api/templates is called with a new template")]
    protected override async Task WhenAsync()
    {
        _response = await Client.PostAsJsonAsync("/api/templates", new
        {
            Name = "ServiceClass",
            IsActive = true,
        });
    }

    [Fact(DisplayName = "UAC01: Response is 201 Created")]
    [Trait(Spec.UAC, "01")]
    [Then("the response status code is 201")]
    public async Task UAC01_ResponseIs201()
    {
        await ArrangeAndActAsync();
        _response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "UAC02: Template persisted in database")]
    [Trait(Spec.UAC, "02")]
    [Then("the database contains a template named ServiceClass")]
    public async Task UAC02_PersistedInDatabase()
    {
        await ArrangeAndActAsync();

        await using var database = CreateDbContext();
        var template = await database.Templates.FirstOrDefaultAsync(row => row.Name == "ServiceClass");
        template.Should().NotBeNull();
    }
}
#endif
