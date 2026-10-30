#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template API CRUD operations")]
public sealed class SC02_CreateTemplate : TemplateApiScenario
{
    private HttpResponseMessage _response = null!;
    private bool _persisted;

    [Given("a template library seeded with two templates")]
    private Task GivenAsync() => SeedLibraryAsync();

    [When("POST /api/templates is called with a new template")]
    private async Task WhenAsync()
    {
        _response = await Client.PostAsJsonAsync("/api/templates", new { Name = "ServiceClass", IsActive = true });

        await using var database = CreateDbContext();
        _persisted = await database.Templates.AnyAsync(row => row.Name == "ServiceClass");
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await GivenAsync();
        await WhenAsync();
    }

    [Then("the response status should be Created")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ResponseIs201() => _response.StatusCode.Should().Be(HttpStatusCode.Created);

    [Then("the template should be persisted")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_PersistedInDatabase() => _persisted.Should().BeTrue();
}
#endif
