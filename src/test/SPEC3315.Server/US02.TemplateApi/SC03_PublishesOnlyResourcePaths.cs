#if NET10_0_OR_GREATER
using System.Net.Http.Json;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "03")]
[UserStory("02", "The server serves the template contract over HTTP")]
public sealed class SC03_PublishesOnlyResourcePaths : IAsyncLifetime, IClassFixture<UseCases.TemplateApiUseCase>
{
    private readonly UseCases.TemplateApiUseCase useCase;
    private TemplateApiFactory _development = null!;

    public SC03_PublishesOnlyResourcePaths(UseCases.TemplateApiUseCase useCase) => this.useCase = useCase;

    [Given("a served template library in development")]
    private void Given() => _development = new TemplateApiFactory();

    [When("the published OpenAPI document is read")]
    private async Task WhenAsync()
    {
        var openApi = await _development.CreateClient().GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        useCase.Cases["PluginPaths"] = openApi.GetProperty("paths")
            .EnumerateObject()
            .Select(path => path.Name)
            .Where(name => name.Contains("tinytools", StringComparison.OrdinalIgnoreCase))
            .ToList();
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

    [Then("the plugin prefixed paths should be empty")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_OpenApiOmitsThePluginIdentity() => ((List<string>)useCase.Cases["PluginPaths"]!).Should().BeEmpty();
}
#endif
