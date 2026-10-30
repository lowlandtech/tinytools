#if NET10_0_OR_GREATER
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "04")]
[UserStory("02", "The server serves the template contract over HTTP")]
public sealed class SC04_PublishesResourceLocation : IAsyncLifetime, IClassFixture<UseCases.TemplateApiUseCase>
{
    private readonly UseCases.TemplateApiUseCase useCase;
    private TemplateApiFactory _development = null!;

    public SC04_PublishesResourceLocation(UseCases.TemplateApiUseCase useCase) => this.useCase = useCase;

    [Given("a served template library in development")]
    private void Given() => _development = new TemplateApiFactory();

    [When("a template is created")]
    private async Task WhenAsync()
    {
        var created = await _development.CreateClient().PostAsJsonAsync("/api/templates", new { Name = "EntityClass" });
        useCase.Cases["Location"] = created.Headers.Location?.ToString();
        useCase.Cases["CreatedId"] = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    public ValueTask InitializeAsync()
    {
        useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult());
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _development?.Dispose();
        return ValueTask.CompletedTask;
    }

    [Then("the location should be canonical")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_LocationUsesTheResourcePath() => useCase.Cases["Location"].Should().Be($"/api/templates/{useCase.Cases["CreatedId"]}");
}
#endif
