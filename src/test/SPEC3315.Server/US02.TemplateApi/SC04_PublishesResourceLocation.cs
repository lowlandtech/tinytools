#if NET10_0_OR_GREATER
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "04")]
[UserStory("02", "The server serves the template contract over HTTP")]
public sealed class SC04_PublishesResourceLocation : IAsyncLifetime
{
    private TemplateApiFactory _development = null!;
    private string? _location;
    private Guid _createdId;

    [Given("a served template library in development")]
    private void Given() => _development = new TemplateApiFactory();

    [When("a template is created")]
    private async Task WhenAsync()
    {
        var created = await _development.CreateClient().PostAsJsonAsync("/api/templates", new { Name = "EntityClass" });
        _location = created.Headers.Location?.ToString();
        _createdId = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    public async ValueTask InitializeAsync()
    {
        Given();
        await WhenAsync();
    }

    public ValueTask DisposeAsync()
    {
        _development?.Dispose();
        return ValueTask.CompletedTask;
    }

    [Then("the location should be the created template path")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_LocationUsesTheResourcePath() => _location.Should().Be($"/api/templates/{_createdId}");
}
#endif
