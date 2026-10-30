#if NET10_0_OR_GREATER
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US01.TemplateApi;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template API CRUD operations")]
public sealed class SC02_CreateTemplate : TemplateApiScenario, IClassFixture<UseCases.TemplateApiUseCase>
{
    private readonly UseCases.TemplateApiUseCase useCase;

    public SC02_CreateTemplate(UseCases.TemplateApiUseCase useCase) => this.useCase = useCase;

    [Given("a template library seeded with two templates")]
    private Task GivenAsync() => SeedLibraryAsync();

    [When("POST /api/templates is called with a new template")]
    private async Task WhenAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/templates", new { Name = "ServiceClass", IsActive = true });
        useCase.Cases["Status"] = response.StatusCode;

        await using var database = CreateDbContext();
        useCase.Cases["Persisted"] = await database.Templates.AnyAsync(row => row.Name == "ServiceClass");
    }

    public override ValueTask InitializeAsync()
    {
        useCase.RunOnce(
            () => { base.InitializeAsync(); GivenAsync().GetAwaiter().GetResult(); },
            () => WhenAsync().GetAwaiter().GetResult());
        return ValueTask.CompletedTask;
    }

    [Then("the response status should be Created")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ResponseIs201() => ((HttpStatusCode)useCase.Cases["Status"]!).Should().Be(HttpStatusCode.Created);

    [Then("the template should be persisted")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_PersistedInDatabase() => ((bool)useCase.Cases["Persisted"]!).Should().Be(true);
}
#endif
