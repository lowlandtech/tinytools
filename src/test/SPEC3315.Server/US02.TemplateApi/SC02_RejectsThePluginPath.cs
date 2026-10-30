#if NET10_0_OR_GREATER
using System.Net;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "02")]
[UserStory("02", "The server serves the template contract over HTTP")]
public sealed class SC02_RejectsThePluginPath : IAsyncLifetime, IClassFixture<UseCases.TemplateApiUseCase>
{
    private readonly UseCases.TemplateApiUseCase useCase;
    private TemplateApiFactory _development = null!;

    public SC02_RejectsThePluginPath(UseCases.TemplateApiUseCase useCase) => this.useCase = useCase;

    [Given("a served template library in development")]
    private void Given() => _development = new TemplateApiFactory();

    [When("the obsolete plugin prefixed template path is requested")]
    private async Task WhenAsync() => useCase.Cases["Status"] = (await _development.CreateClient().GetAsync("/api/tinytools/templates")).StatusCode;

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

    [Then("the plugin path status should be NotFound")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_ObsoletePathDoesNotDispatch() => ((HttpStatusCode)useCase.Cases["Status"]!).Should().Be(HttpStatusCode.NotFound);
}
#endif
