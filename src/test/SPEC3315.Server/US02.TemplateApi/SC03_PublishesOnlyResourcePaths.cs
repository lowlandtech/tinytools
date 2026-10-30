#if NET10_0_OR_GREATER
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "03")]
[UserStory("02", "The server serves the template contract over HTTP")]
public sealed class SC03_PublishesOnlyResourcePaths : IAsyncLifetime
{
    private TemplateApiFactory _development = null!;
    private List<string> _pluginPaths = [];

    [Given("a served template library in development")]
    private void Given() => _development = new TemplateApiFactory();

    [When("the published OpenAPI document is read")]
    private async Task WhenAsync()
    {
        var openApi = await _development.CreateClient().GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        _pluginPaths = openApi.GetProperty("paths")
            .EnumerateObject()
            .Select(path => path.Name)
            .Where(name => name.Contains("tinytools", StringComparison.OrdinalIgnoreCase))
            .ToList();
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

    [Then("the plugin prefixed paths should be empty")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_OpenApiOmitsThePluginIdentity() => _pluginPaths.Should().BeEmpty();
}
#endif
